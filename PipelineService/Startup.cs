using System;
using System.Threading.Tasks;
using System.Transactions;
using Hangfire;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neo4jClient;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Extensions;
using PipelineService.Filters;
using PipelineService.Helper;
using PipelineService.Models;
using PipelineService.Services;
using PipelineService.Services.HealthChecks;
using PipelineService.Services.Impl;

namespace PipelineService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			// TODO this should be stricter
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAnyCorsPolicy",
					policy => policy
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader());
			});

			// configuring Neo4j connection
			services.AddSingleton<IGraphClient>(new GraphClient(
				Configuration.GetValueOrThrow<Uri>("NeoServerConfiguration:RootUri"),
				Configuration.GetValueOrThrow<string>("NeoServerConfiguration:Username"),
				Configuration.GetValueOrThrow<string>("NeoServerConfiguration:Password"))
			{
				DefaultDatabase = Configuration.GetValue("NeoServerConfiguration:DefaultDatabase", "neo4j")
			});
			var defaultMySqlConnectionString = Configuration.GetConnectionString("DefaultConnection");

			// external services
			services.AddMemoryCache();
			services.AddHttpClient();

			// Add Hangfire services.
			services.AddHangfire(configuration =>
			{
				configuration
					.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
					.UseSimpleAssemblyNameTypeSerializer()
					.UseRecommendedSerializerSettings();
				var storageOptions = new MySqlStorageOptions
				{
					TransactionIsolationLevel = IsolationLevel.ReadCommitted,
					QueuePollInterval = TimeSpan.FromSeconds(15),
					JobExpirationCheckInterval = TimeSpan.FromHours(1),
					CountersAggregateInterval = TimeSpan.FromMinutes(5),
					PrepareSchemaIfNecessary = true,
					DashboardJobListLimit = 50000
				};
				configuration.UseStorage(
					new MySqlStorage(Configuration.GetConnectionString("HangfireConnection"), storageOptions));
			});

			// Add the processing server as IHostedService
			services.AddHangfireServer();

			// databases
			services.AddNeo4jAnnotations<PipelineGraphContext>();
			services.AddDbContext<EfDatabaseContext>(options =>
			{
				options.UseMySql(
					defaultMySqlConnectionString,
					new MySqlServerVersion(new Version(Configuration.GetValue("MySqlServerVersion", "8.0.29"))));
			}, ServiceLifetime.Transient);

			// Registering singleton services
			services.AddSingleton<EventBusService>();
			services.AddSingleton<EdgeEventBusService>();

			// Registering DAOs
			services.AddTransient<IPipelinesExecutionDao, PipelinesExecutionDao>();
			services.AddSingleton<IPipelinesDaoInMemory, InMemoryPipelinesDaoInMemory>();
			services.AddSingleton<IPipelinesDao, Neo4JPipelinesDao>();
			services.AddSingleton<IPipelineCandidateDao, PipelineCandidateDaoFileSystem>();

			// Registering transient services
			services.AddTransient<IHashService, HashService>();
			services.AddTransient<IPipelineExecutionService, PipelinesExecutionService>();
			services.AddTransient<IOperationsService, OperationsService>();
			services.AddTransient<IPipelinesDtoService, PipelinesDtoService>();
			services.AddTransient<IOperationTemplatesService, OperationTemplatesService>();
			services.AddTransient<IPipelineCandidateService, PipelineCandidateService>();
			services.AddTransient<IMetricsService, MetricsService>();
			services.AddTransient<ILearningServiceClient, LearningServiceClient>();
			services.AddTransient<IDatasetServiceClient, DatasetServiceClient>();

			services.AddHostedService<HostedSubscriptionService>();

			// TODO: Add health checks to required services: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
			services.AddHealthChecks()
				.AddCheck<Neo4JHealthCheck>("neo4j_health_check")
				.AddMySql(connectionString: defaultMySqlConnectionString, name: "mysql_health_check");

			services.AddControllers();
			services.AddSwaggerGen(c =>
			{
				c.SwaggerDoc("v1", new OpenApiInfo { Title = "Pipeline Service", Version = "v1" });
			});
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IPipelinesDao pipelinesDao)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseSwagger();
				app.UseSwaggerUI(c =>
				{
					c.SwaggerEndpoint("/swagger/v1/swagger.json", "PipelineDao v1");
					// swagger UI at root
					c.RoutePrefix = string.Empty;
				});
			}
			else
			{
				app.UseHttpsRedirection();
			}

			app.UseRouting();

			app.UseCors("AllowAnyCorsPolicy");

			app.UseAuthorization();

			if (env.IsProduction())
			{
				// fix hangfire problem when deployed behind proxy (nginx forwarding)
				// https://github.com/HangfireIO/Hangfire/issues/1368
				app.Use((context, next) =>
				{
					var pathBase = new PathString(context.Request.Headers["X-Forwarded-Prefix"]);
					if (pathBase != null)
						context.Request.PathBase = new PathString(pathBase.Value);
					return next();
				});
			}

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
				endpoints.MapHangfireDashboard(new DashboardOptions()
				{
					Authorization = new[] { new HangfireAuthorizationFilter() }
				});
			});

			Task.WhenAll(pipelinesDao.Setup());
			HandySelfMigrator.Migrate<EfDatabaseContext>(app);

			if (Configuration.GetValue("ScheduledCandidateProcessing", false))
			{
				// schedule recurring jobs
				RecurringJob.AddOrUpdate<IPipelinesDtoService>(
					"candidate_processing",
					s => s.ProcessPipelineCandidates(
						Configuration.GetValue("CandidateProcessing:CandidatesPerBatch", 30)), Cron.Hourly);
				BackgroundJob.Enqueue<IPipelinesDtoService>(s => s.ProcessIncompleteCandidatesInBackground());
			}
		}
	}
}
