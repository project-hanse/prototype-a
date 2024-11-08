using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
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
		var raw = "";
		foreach (var file in files)
		{
			string path = null;
			try
			{
				path = Path.Combine(PipelineCandidatesPath, file);
				raw = (await File.ReadAllTextAsync(path)).Trim();
				var candidate = JsonConvert.DeserializeObject<PipelineCandidate>(raw);
				if (candidate == null)
				{
					throw new SerializationException("Failed to deserialize pipeline candidate");
				}

				candidate.SourceFileName = file;
				pipelineCandidates.Add(candidate);
			}
			catch (SerializationException e)
			{
				_logger.LogInformation(
					"Failed to deserialize pipeline candidate from file {CandidateFile}", file);
				if (path != null)
				{
					_logger.LogInformation("Deleting file {CandidateFile}", file);
					File.Delete(path);
				}
			}
			catch (Exception e)
			{
				_logger.LogError(
					"Failed to load pipeline candidate from file {CandidateFile} - [{ErrorType}] {ErrorMessage} \nRaw: '{Raw}'",
					file, e.GetType().Name, e.Message, raw);

				if (path != null)
				{
					_logger.LogInformation("Deleting corrupted file {CandidateFile}", file);
					File.Delete(path);
				}

				continue;
			}
		}

		_logger.LogInformation("Loaded {CandidateCount} pipeline candidates from file system", pipelineCandidates.Count);

		return pipelineCandidates;
	}

	public async Task<bool> DeletePipelineCandidate(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Deleting pipeline candidate ({CandidateId}) from file system...", pipelineCandidateId);
		var candidates = await GetPipelineCandidates();
		var candidate = candidates.FirstOrDefault(c => c.PipelineId == pipelineCandidateId);
		if (candidate == null)
		{
			_logger.LogInformation(
				"Failed to delete pipeline candidate ({CandidateId}) from file system - candidate not found",
				pipelineCandidateId);
			return false;
		}

		var path = Path.Combine(PipelineCandidatesPath, candidate.SourceFileName);
		if (!File.Exists(path))
		{
			_logger.LogInformation("Failed to delete pipeline candidate ({CandidateId}) from file system - file not found",
				pipelineCandidateId);
			return false;
		}

		File.Delete(path);
		_logger.LogInformation("Deleted pipeline candidate ({CandidateId}) from file system", pipelineCandidateId);
		return true;
	}

	public async Task<bool> ArchivePipelineCandidate(Guid pipelineCandidateId)
	{
		_logger.LogDebug("Archiving pipeline candidate in file system...");
		var candidates = await GetPipelineCandidates();
		var candidate = candidates.FirstOrDefault(c => c.PipelineId == pipelineCandidateId);
		if (candidate == null)
		{
			_logger.LogInformation(
				"Failed to archive pipeline candidate ({CandidateId}) from file system - candidate not found",
				pipelineCandidateId);
			return false;
		}

		var path = Path.Combine(PipelineCandidatesPath, candidate.SourceFileName);
		if (!File.Exists(path))
		{
			_logger.LogInformation("Failed to archive pipeline candidate ({CandidateId}) from file system - file not found",
				pipelineCandidateId);
			return false;
		}

		if (!Directory.Exists(PipelineCandidatesArchivePath))
		{
			_logger.LogInformation("Creating archive folder {ArchiveFolder}...", PipelineCandidatesArchivePath);
			Directory.CreateDirectory(PipelineCandidatesArchivePath);
		}

		File.Move(path, Path.Combine(PipelineCandidatesArchivePath, Path.GetFileName(path)));
		_logger.LogInformation("Archived pipeline candidate ({CandidateId}) in file system", pipelineCandidateId);
		return true;
	}
}
