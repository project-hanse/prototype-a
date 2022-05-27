using System;
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

	public async Task<PaginatedList<PipelineCandidate>> GetPipelineCandidateDtos(Pagination pagination)
	{
		_logger.LogDebug("Loading available pipeline candidates...");

		// TODO make this more efficient (not in memory)
		var candidates = await _pipelineCandidateDao.GetPipelineCandidates();

		pagination.Sort = pagination.Sort?.Trim() ?? "";
		if (!string.IsNullOrEmpty(pagination.Sort))
		{
			pagination.Sort = string.Concat(pagination.Sort.FirstOrDefault().ToString().ToUpper(), pagination.Sort.AsSpan(1));
		}

		var sortProperty = typeof(PipelineCandidate).GetProperty(pagination.Sort);
		if (sortProperty == null)
		{
			sortProperty = typeof(PipelineCandidate).GetProperty(nameof(PipelineCandidate.CompletedAt));
		}

		if (sortProperty == null)
		{
			throw new ArgumentException($"{pagination.Sort} is not a valid sort property");
		}

		candidates = pagination.Order == "asc"
			? candidates.OrderBy(x => sortProperty.GetValue(x)).ToList()
			: candidates.OrderByDescending(x => sortProperty.GetValue(x)).ToList();

		var response = new PaginatedList<PipelineCandidate>
		{
			TotalItems = candidates.Count,
			Items = candidates.Skip(pagination.PageSize * (pagination.Page - 1)).Take(pagination.PageSize).ToList()
		};

		foreach (var pipelineCandidate in response.Items)
		{
			pipelineCandidate.Actions = null;
		}

		_logger.LogInformation("Loaded {PipelineCandidateCount} pipeline candidates", candidates.Count);

		return response;
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
