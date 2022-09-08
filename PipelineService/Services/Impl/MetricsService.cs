using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Metrics;

namespace PipelineService.Services.Impl;

public class MetricsService : IMetricsService
{
	private readonly ILogger<MetricsService> _logger;
	private readonly EfDatabaseContext _databaseContext;

	public MetricsService(ILogger<MetricsService> logger, EfDatabaseContext databaseContext)
	{
		_logger = logger;
		_databaseContext = databaseContext;
	}

	public async Task<PaginatedList<CandidateProcessingMetric>> GetCandidateProcessingMetrics(Pagination pagination)
	{
		_logger.LogDebug("Loading candidate processing metrics");

		if (string.IsNullOrEmpty(pagination.Sort?.Trim()))
		{
			pagination.Sort = nameof(CandidateProcessingMetric.CreatedOn);
		}

		var sortProperty = typeof(CandidateProcessingMetric).GetProperty(pagination.Sort);

		if (sortProperty == null)
		{
			throw new ArgumentException($"{pagination.Sort} is not a valid sort property");
		}

		var candidateProcessingMetrics = await (pagination.Order == "asc"
				? _databaseContext.CandidateProcessingMetrics.OrderBy(x => x.CreatedOn)
				: _databaseContext.CandidateProcessingMetrics.OrderByDescending(x => x.CreatedOn))
			.Skip(pagination.Page * pagination.PageSize)
			.Take(pagination.PageSize)
			.ToListAsync();

		_logger.LogInformation("Loaded {CandidateProcessingMetricCount} candidate processing metrics",
			candidateProcessingMetrics.Count);

		return new PaginatedList<CandidateProcessingMetric>
		{
			TotalItems = await _databaseContext.CandidateProcessingMetrics.CountAsync(),
			Page = pagination.Page,
			PageSize = pagination.PageSize,
			Items = candidateProcessingMetrics
		};
	}
}
