using Microsoft.EntityFrameworkCore;
using PipelineService.Models.Metrics;

namespace PipelineService.Models;

public class EfMetricsContext : DbContext
{
	public EfMetricsContext(DbContextOptions<EfMetricsContext> options) : base(options)
	{
	}

	public DbSet<CandidateProcessingMetric> CandidateProcessingMetrics { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CandidateProcessingMetric>().ToTable(nameof(CandidateProcessingMetrics));
	}
}
