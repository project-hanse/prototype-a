using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Dao;

public interface IPipelineCandidateDao
{
	Task<IList<PipelineCandidate>> GetPipelineCandidates();
	Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId);
}
