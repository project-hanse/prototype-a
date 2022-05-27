using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services;

public interface IPipelineCandidateService
{
	/// <summary>
	/// Loads all pipeline candidates, but sets actions to null to reduce object size.
	/// </summary>
	Task<IList<PipelineCandidate>> GetPipelineCandidateDtos();


	Task<PipelineCandidate> GetCandidateById(Guid pipelineCandidateId);

	Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId);
}
