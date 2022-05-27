using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Models.Dtos;

namespace PipelineService.Services.Impl;

public class PipelineCandidateService : IPipelineCandidateService
{
	private readonly ILogger<PipelineCandidateService> _logger;
	private readonly IPipelineCandidateDao _pipelineCandidateDao;

	public PipelineCandidateService(ILogger<PipelineCandidateService> logger, IPipelineCandidateDao pipelineCandidateDao)
	{
		_logger = logger;
		_pipelineCandidateDao = pipelineCandidateDao;
	}

	public async Task<IList<PipelineCandidate>> GetPipelineCandidateDtos()
	{
		_logger.LogDebug("Loading available pipeline candidates...");

		var candidates = await _pipelineCandidateDao.GetPipelineCandidates();

		foreach (var pipelineCandidate in candidates)
		{
			pipelineCandidate.Actions = null;
		}

		_logger.LogInformation("Loaded {PipelineCandidateCount} pipeline candidates", candidates.Count);

		return candidates;
	}

	public async Task<PipelineCandidate> GetCandidateById(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Loading pipeline candidate with id {PipelineCandidateId}", pipelineCandidateId);
		// TODO make this more efficient
		var candidates = await _pipelineCandidateDao.GetPipelineCandidates();
		var candidate = candidates.FirstOrDefault(c => c.PipelineId == pipelineCandidateId);
		if (candidate == default)
		{
			_logger.LogWarning("Pipeline candidate with id {PipelineCandidateId} not found", pipelineCandidateId);
			return null;
		}

		_logger.LogInformation("Loaded pipeline candidate with id {PipelineCandidateId}", pipelineCandidateId);
		return candidate;
	}

	public async Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Deleting pipeline candidate with id {PipelineCandidateId}", pipelineCandidateId);
		return await _pipelineCandidateDao.DeletePipelineCandidate(pipelineCandidateId);
	}
}
