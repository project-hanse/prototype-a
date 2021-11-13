using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Neo4jClient;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Extensions;
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
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAnyCorsPolicy",
					policy => policy
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader());
			});

			// Configuring Hangfire services
			services.AddHangfire(configuration =>
			{
				configuration
					.UseSimpleAssemblyNameTypeSerializer()
					.UseRecommendedSerializerSettings()
					.UseLiteDbStorage("hangfire.db");
			});
			services.AddHangfireServer();

			// configuring Neo4j connection
			services.AddSingleton<IGraphClient>(new GraphClient(
				Configuration.GetValueOrThrow<Uri>("NeoServerConfiguration:RootUri"),
				Configuration.GetValueOrThrow<string>("NeoServerConfiguration:Username"),
				Configuration.GetValueOrThrow<string>("NeoServerConfiguration:Password"))
			{
				DefaultDatabase = Configuration.GetValue("NeoServerConfiguration:DefaultDatabase", "neo4j")
			});

			services.AddNeo4jAnnotations<PipelineContext>();

			// Registering singleton services
			services.AddSingleton<EventBusService>();
			services.AddSingleton<EdgeEventBusService>();

			// Registering DAOs
			services.AddSingleton<IPipelinesExecutionDao, InMemoryPipelinesExecutionDao>();
			services.AddSingleton<IPipelinesDaoInMemory, InMemoryPipelinesDaoInMemory>();
			services.AddSingleton<IPipelinesDao, Neo4JPipelinesDao>();

			// Registering transient services
			services.AddTransient<IHashService, HashService>();
			services.AddTransient<IPipelineExecutionService, PipelinesExecutionService>();
			services.AddTransient<INodesService, NodesService>();
			services.AddTransient<IPipelinesDtoService, PipelinesDtoService>();
			services.AddTransient<IOperationsService, OperationsService>();

			services.AddHostedService<HostedSubscriptionService>();

			// TODO: Add health checks to required services: https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
			services.AddHealthChecks().AddCheck<Neo4JHealthCheck>("neo4j_health_check");

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
				app.UseHangfireDashboard();
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
			});

			Task.WhenAll(pipelinesDao.Setup());
		}
	}
}
