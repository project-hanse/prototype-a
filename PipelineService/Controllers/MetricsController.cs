using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PipelineService.Models.Dtos;
using PipelineService.Models.Metrics;
using PipelineService.Services;

namespace PipelineService.Controllers;

public class MetricsController : BaseController
{
	private readonly IMetricsService _metricsService;

	public MetricsController(IMetricsService metricsService)
	{
		_metricsService = metricsService;
	}

	[HttpGet("processing/candidates")]
	public async Task<PaginatedList<CandidateProcessingMetric>> GetCandidateProcessingMetrics([FromQuery] Pagination pagination)
	{
		return await _metricsService.GetCandidateProcessingMetrics(pagination);
	}
}
