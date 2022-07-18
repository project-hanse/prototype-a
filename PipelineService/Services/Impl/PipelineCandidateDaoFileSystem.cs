using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Models.Dtos;

namespace PipelineService.Services.Impl;

public class PipelineCandidateDaoFileSystem : IPipelineCandidateDao
{
	private readonly ILogger<PipelineCandidateDaoFileSystem> _logger;
	private readonly IConfiguration _configuration;

	private string PipelineCandidatesPath => Path.Combine(
		_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
		_configuration.GetValue("PipelineCandidatesFolder", ""));

	private string PipelineCandidatesArchivePath => Path.Combine(
		_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
		_configuration.GetValue("PipelineCandidatesArchiveFolder", ""));

	public PipelineCandidateDaoFileSystem(ILogger<PipelineCandidateDaoFileSystem> logger, IConfiguration configuration)
	{
		_logger = logger;
		_configuration = configuration;
	}

	public Task<int> GetPipelineCandidatesTotal()
	{
		_logger.LogDebug("Counting pipeline candidates");
		var candidates = Directory.GetFiles(PipelineCandidatesPath);
		return Task.FromResult(candidates.Length);
	}

	public async Task<IList<PipelineCandidate>> GetPipelineCandidates()
	{
		_logger.LogDebug("Loading pipeline candidates from file system...");

		var files = Directory.GetFiles(PipelineCandidatesPath);
		var pipelineCandidates = new List<PipelineCandidate>();
		foreach (var file in files)
		{
			try
			{
				var candidate = JsonConvert.DeserializeObject<PipelineCandidate>(
					await File.ReadAllTextAsync(Path.Combine(PipelineCandidatesPath, file)));
				if (candidate == null)
				{
					throw new Exception("Failed to deserialize pipeline candidate");
				}

				candidate.SourceFileName = file;
				pipelineCandidates.Add(candidate);
			}
			catch (Exception e)
			{
				_logger.LogInformation("Failed to load pipeline candidate from file {CandidateFile} - {ErrorMessage}",
					file, e.Message);
				continue;
			}
		}

		_logger.LogInformation("Loaded {CandidateCount} pipeline candidates from file system", pipelineCandidates.Count);

		return pipelineCandidates;
	}

	public async Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Deleting pipeline candidate from file system...");
		var candidates = await GetPipelineCandidates();
		var candidate = candidates.FirstOrDefault(c => c.PipelineId == pipelineCandidateId);
		if (candidate == null)
		{
			_logger.LogInformation("Failed to delete pipeline candidate from file system - candidate not found");
			return false;
		}

		var path = Path.Combine(PipelineCandidatesPath, candidate.SourceFileName);
		if (!File.Exists(path))
		{
			_logger.LogInformation("Failed to delete pipeline candidate from file system - file not found");
			return false;
		}

		File.Delete(path);
		_logger.LogInformation("Deleted pipeline candidate from file system");
		return true;
	}

	public async Task<bool> ArchivePipelineCandidate(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Archiving pipeline candidate in file system...");
		var candidates = await GetPipelineCandidates();
		var candidate = candidates.FirstOrDefault(c => c.PipelineId == pipelineCandidateId);
		if (candidate == null)
		{
			_logger.LogInformation("Failed to archive pipeline candidate from file system - candidate not found");
			return false;
		}

		var path = Path.Combine(PipelineCandidatesPath, candidate.SourceFileName);
		if (!File.Exists(path))
		{
			_logger.LogInformation("Failed to archive pipeline candidate from file system - file not found");
			return false;
		}

		if (!Directory.Exists(PipelineCandidatesArchivePath))
		{
			_logger.LogInformation("Creating archive folder {ArchiveFolder}...", PipelineCandidatesArchivePath);
			Directory.CreateDirectory(PipelineCandidatesArchivePath);
		}

		File.Move(path, Path.Combine(PipelineCandidatesArchivePath, Path.GetFileName(path)));
		_logger.LogInformation("Archived pipeline candidate in file system");
		return true;
	}
}
