using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services;

public interface IPipelineCandidateService
{
	/// <summary>
	/// Loads all pipeline candidates.
	/// </summary>
	/// <param name="pagination"></param>
	public Task<IList<PipelineCandidate>> GetPipelineCandidates(Pagination pagination);

	/// <summary>
	/// Loads all pipeline candidates, but sets actions to null to reduce object size.
	/// </summary>
	/// <param name="pagination"></param>
	Task<PaginatedList<PipelineCandidate>> GetPipelineCandidateDtos(Pagination pagination);

	/// <summary>
	/// Loads a pipeline candidate by id.
	/// </summary>
	/// <param name="pipelineCandidateId">The candidate's id.</param>
	Task<PipelineCandidate> GetCandidateById(Guid pipelineCandidateId);

	/// <summary>
	/// Delete a pipeline candidate by id.
	/// </summary>
	/// <param name="pipelineCandidateId">The candidate's id.</param>
	/// <returns>True if the candidate has been deleted, else false.</returns>
	Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId);
}
