using System;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services;

public interface IPipelineCandidateService
{
	/// <summary>
	/// Loads all pipeline candidates, but sets actions to null to reduce object size.
	/// </summary>
	/// <param name="pagination"></param>
	Task<PaginatedList<PipelineCandidate>> GetPipelineCandidateDtos(Pagination pagination);


	Task<PipelineCandidate> GetCandidateById(Guid pipelineCandidateId);

	Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId);
}
