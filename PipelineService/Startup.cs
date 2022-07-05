using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Transactions;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neo4jClient;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Extensions;
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
			services.AddDbContext<EfMetricsContext>(options =>
			{
				options.UseMySql(
					defaultMySqlConnectionString,
					new MySqlServerVersion(new Version(Configuration.GetValue("MySqlServerVersion", "5.7.38"))));
			});

			// Registering singleton services
			services.AddSingleton<EventBusService>();
			services.AddSingleton<EdgeEventBusService>();

			// Registering DAOs
			services.AddSingleton<IPipelinesExecutionDao, InMemoryPipelinesExecutionDao>();
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

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
				endpoints.MapHangfireDashboard(new DashboardOptions()
				{
					Authorization = new List<IDashboardAuthorizationFilter>()
				});
			});

			Task.WhenAll(pipelinesDao.Setup());
			HandySelfMigrator.Migrate<EfMetricsContext>(app);

			// schedule recurring jobs
			BackgroundJob.Schedule<IPipelinesDtoService>(s => s.ProcessPipelineCandidates(
					Configuration.GetValue("CandidateProcessing:CandidatesPerBatch", 30)),
				TimeSpan.FromMinutes(Configuration.GetValue("CandidateProcessing:Interval", 15)));
		}
	}
}
