using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Metrics;

namespace PipelineService.Services;

public interface IMetricsService
{
	Task<PaginatedList<CandidateProcessingMetric>> GetCandidateProcessingMetrics(Pagination pagination);
}
