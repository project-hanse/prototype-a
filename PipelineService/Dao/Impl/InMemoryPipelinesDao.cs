using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
	public class InMemoryPipelinesDao : IPipelinesDao
	{
		private readonly ILogger<InMemoryPipelinesDao> _logger;
		private static readonly IDictionary<Guid, Pipeline> Store = new ConcurrentDictionary<Guid, Pipeline>();

		public InMemoryPipelinesDao(ILogger<InMemoryPipelinesDao> logger)
		{
			_logger = logger;
		}

		public async Task<IList<Pipeline>> CreateDefaults(IList<Pipeline> pipelines = null)
		{
			var newDefaultPipelines = pipelines ?? HardcodedDefaultPipelines.NewDefaultPipelines();

			_logger.LogInformation("Creating {NewPipelines} new pipeline(s)", newDefaultPipelines.Count);

			foreach (var pipeline in newDefaultPipelines)
			{
				await Add(pipeline);
			}

			return newDefaultPipelines;
		}

		public Task Add(Pipeline pipeline)
		{
			if (!Store.ContainsKey(pipeline.Id))
			{
				Store.Add(pipeline.Id, pipeline);
				_logger.LogDebug("Added new pipeline with id {PipelineId}", pipeline.Id);
			}
			else
			{
				_logger.LogWarning("Pipeline with id {PipelineId} already exists", pipeline.Id);
			}

			return Task.CompletedTask;
		}

		public Task<Pipeline> Get(Guid pipelineId)
		{
			if (!Store.TryGetValue(pipelineId, out var pipeline))
			{
				throw new NotFoundException("No pipeline with given id found");
			}

			_logger.LogInformation("Loading pipeline with id {PipelineId}", pipelineId);
			return Task.FromResult(pipeline);
		}

		public Task<PipelineInfoDto> GetInfoDto(Guid pipelineId)
		{
			if (!Store.TryGetValue(pipelineId, out var pipeline))
			{
				throw new NotFoundException("No pipeline with given id found");
			}

			return Task.FromResult(MapToDto(pipeline));
		}

		private static PipelineInfoDto MapToDto(Pipeline pipeline)
		{
			return new PipelineInfoDto
			{
				Id = pipeline.Id,
				Name = pipeline.Name,
				CreatedOn = pipeline.CreatedOn
			};
		}

		public async Task<IList<Pipeline>> Get()
		{
			if (Store.Count == 0)
			{
				_logger.LogInformation("Creating new default pipelines");
				await CreateDefaults();
			}

			var pipelines = Store
				.Select(r => r.Value)
				.ToList();

			_logger.LogInformation("Loading {PipelineCount} pipeline(s)", pipelines.Count);

			return pipelines;
		}

		public async Task<IList<PipelineInfoDto>> GetDtos()
		{
			var pipelines = await Get();
			return pipelines
				.OrderByDescending(p => p.CreatedOn)
				.Select(MapToDto)
				.ToList();
		}

		public Task<Pipeline> Update(Pipeline pipeline)
		{
			_logger.LogDebug("Updating pipeline {PipelineId}", pipeline.Id);
			pipeline.ChangedOn = DateTime.UtcNow;

			if (!Store.ContainsKey(pipeline.Id))
			{
				_logger.LogWarning(
					"Pipeline {PipelineId} does not exist in store, but update was called - storing it anyways",
					pipeline.Id);
			}
			else
			{
				Store.Remove(pipeline.Id);
			}

			Store.Add(pipeline.Id, pipeline);
			return Task.FromResult(pipeline);
		}
	}
}
