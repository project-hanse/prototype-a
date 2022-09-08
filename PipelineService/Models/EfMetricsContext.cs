using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using PipelineService.Models.Metrics;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Models;

public class EfMetricsContext : DbContext
{
	public EfMetricsContext(DbContextOptions<EfMetricsContext> options) : base(options)
	{
	}

	public DbSet<CandidateProcessingMetric> CandidateProcessingMetrics { get; set; }
	// public DbSet<PipelineExecutionRecord> PipelineExecutionRecords { get; set; }
	// public DbSet<OperationExecutionRecord> OperationExecutionRecords { get; set; }
	// public DbSet<Dataset> Datasets { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		var valueComparer = new ValueComparer<IDictionary<int, int>>(
			(c1, c2) => c1.SequenceEqual(c2),
			c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
			c => c);
		modelBuilder
			.Entity<CandidateProcessingMetric>()
			.Property(b => b.OperationsRandomizedCount)
			.HasConversion(
				v => JsonConvert.SerializeObject(v),
				v => JsonConvert.DeserializeObject<Dictionary<int, int>>(v))
			.Metadata.SetValueComparer(valueComparer);
	}
}
