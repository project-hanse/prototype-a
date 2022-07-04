using Microsoft.Extensions.Logging;

namespace PipelineService.Models.Initializer;

public static class DbInitializer
{
	public static void Initialize(EfMetricsContext context, ILogger logger)
	{
		logger.LogInformation("Ensure database exists...");

		var dbCreated = context.Database.EnsureCreated();
		if (dbCreated)
		{
			logger.LogInformation("Created database for {DbContextName}", context.GetType().Name);
		}
		else
		{
			logger.LogInformation("Database {DbContextName} already exists", context.GetType().Name);
		}

		context.SaveChanges();
	}
}
