using Microsoft.EntityFrameworkCore;
using PipelineService.Models.Metrics;

namespace PipelineService.Models;

public class EfMetricsContext : DbContext
{
	public EfMetricsContext(DbContextOptions<EfMetricsContext> options) : base(options)
	{
	}

	public DbSet<CandidateImportMetric> CandidateImportMetrics { get; set; }

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		modelBuilder.Entity<CandidateImportMetric>().ToTable(nameof(CandidateImportMetrics));
	}
}
