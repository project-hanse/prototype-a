using Amazon.S3;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace FileService
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
			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileService", Version = "v1" }); });

			var config = new AmazonS3Config
			{
				ForcePathStyle = Configuration.GetValue("S3Configuration:ForcePathStyle", true),
				ServiceURL =
					$"{Configuration.GetValue("S3Configuration:Protocol", "http")}://{Configuration.GetValue("S3Configuration:Host", "localstack-s3")}:{Configuration.GetValue("S3Configuration:Port", 4566)}"
			};

			services.AddHealthChecks().AddS3(options =>
			{
				options.S3Config = config;
				options.BucketName = Configuration.GetValue("S3Configuration:DefaultBucketName", "defaultfiles");
				options.AccessKey = Configuration.GetValue<string>("S3Configuration:AccessKey", null);
				options.SecretKey = Configuration.GetValue<string>("S3Configuration:SecretKey", null);
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
						c.SwaggerEndpoint("/swagger/v1/swagger.json", "FileService v1");
						// swagger UI at root
						c.RoutePrefix = string.Empty;
					}
				);
			}

			app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
				endpoints.MapHealthChecks("/health");
			});
		}
	}
}
