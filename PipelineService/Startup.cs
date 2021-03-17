using Hangfire;
using Hangfire.LiteDB;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using PipelineService.Dao;
using PipelineService.Dao.Impl;
using PipelineService.Services;
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
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowAnyOrigin());
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

            // Registering singleton services
            services.AddSingleton<IMqttMessageService, MqttMessageService>();

            // Registering DAOs
            services.AddSingleton<IPipelineExecutionDao, InMemoryPipelineExecutionDao>();
            services.AddSingleton<IPipelineDao, InMemoryPipelineDao>();

            // Registering transient services
            services.AddTransient<IHashService, HashService>();
            services.AddTransient<IPipelineExecutionService, PipelineExecutionService>();

            services.AddHostedService<HostedSubscriptionService>();

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo {Title = "PipelineDao", Version = "v1"});
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
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

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors("AllowAnyCorsPolicy");

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}