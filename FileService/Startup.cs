using Amazon.Runtime;
using Amazon.S3;
using FileService.Services;
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
			// TODO this should be stricter
			services.AddCors(options =>
			{
				options.AddPolicy("AllowAnyCorsPolicy",
					policy => policy
						.AllowAnyOrigin()
						.AllowAnyMethod()
						.AllowAnyHeader());
			});

			services.AddControllers();
			services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new OpenApiInfo { Title = "FileService", Version = "v1" }); });

			services.AddScoped<S3FilesService>(); // TODO add interface

			var s3Config = new AmazonS3Config
			{
				ForcePathStyle = Configuration.GetValue("S3Configuration:ForcePathStyle", true),
				ServiceURL =
					$"{Configuration.GetValue("S3Configuration:Protocol", "http")}://{Configuration.GetValue("S3Configuration:Host", "localstack-s3")}:{Configuration.GetValue("S3Configuration:Port", 4566)}"
			};
			var awsCredentials = new BasicAWSCredentials(
				Configuration.GetValue<string>("S3Configuration:AccessKey", null),
				Configuration.GetValue<string>("S3Configuration:SecretKey", null));

			services.AddScoped<IAmazonS3>(_ => new AmazonS3Client(awsCredentials, s3Config));

			services.AddHealthChecks().AddS3(options =>
			{
				options.S3Config = s3Config;
				options.BucketName = Configuration.GetValue("S3Configuration:DefaultBucketName", "defaultfiles");
				options.AccessKey = awsCredentials.GetCredentials().AccessKey;
				options.SecretKey = awsCredentials.GetCredentials().SecretKey;
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

			if (env.IsProduction())
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
		}
	}
}
