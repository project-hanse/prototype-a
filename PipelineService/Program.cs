using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PipelineService.Models;
using PipelineService.Models.Initializer;

namespace PipelineService
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var host = CreateHostBuilder(args).Build();

			CreateDbIfNotExists(host);

			host.Run();
		}

		private static void CreateDbIfNotExists(IHost host)
		{
			using var scope = host.Services.CreateScope();
			var services = scope.ServiceProvider;
			var logger = services.GetRequiredService<ILogger<Program>>();
			try
			{
				var context = services.GetRequiredService<EfMetricsContext>();
				DbInitializer.Initialize(context, logger);
			}
			catch (Exception ex)
			{
				logger.LogError(ex, "An error occurred creating the DB");
			}
		}

		private static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Startup>(); });
	}
}
