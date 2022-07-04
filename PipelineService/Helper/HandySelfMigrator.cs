using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace PipelineService.Helper;

// Based on https://medium.com/@floyd.may/ef-core-app-migrate-on-startup-d046afdba258
public static class HandySelfMigrator
{
	public static void Migrate<TContext>(IApplicationBuilder builder) where TContext : DbContext
	{
		using var scope = builder.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
		using var ctx = scope.ServiceProvider.GetRequiredService<TContext>();

		var sp = ctx.GetInfrastructure();

		var logger = scope.ServiceProvider.GetRequiredService<ILogger<TContext>>();

		logger.LogDebug("Migrating database associated with context {DbContextName}...", typeof(TContext).Name);

		var modelDiffer = sp.GetRequiredService<IMigrationsModelDiffer>();
		var migrationsAssembly = sp.GetRequiredService<IMigrationsAssembly>();

		var modelInitializer = sp.GetRequiredService<IModelRuntimeInitializer>();
		var sourceModel = modelInitializer.Initialize(migrationsAssembly.ModelSnapshot!.Model);

		var designTimeModel = sp.GetRequiredService<IDesignTimeModel>();
		var readOptimizedModel = designTimeModel.Model;

		var diffsExist = modelDiffer.HasDifferences(
			sourceModel.GetRelationalModel(),
			readOptimizedModel.GetRelationalModel());

		if (diffsExist)
		{
			throw new InvalidOperationException(
				"There are differences between the current database model and the most recent migration.");
		}

		ctx.Database.Migrate();
		logger.LogInformation("Database associated with context {DbContextName} migrated", typeof(TContext).Name);
	}
}
