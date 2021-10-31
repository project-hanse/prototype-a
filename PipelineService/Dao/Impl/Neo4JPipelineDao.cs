using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.DataAnnotations;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
	public class Neo4JPipelineDao : IPipelinesDao
	{
		private readonly ILogger<Neo4JPipelineDao> _logger;
		private readonly IGraphClient _graphClient;

		public Neo4JPipelineDao(ILogger<Neo4JPipelineDao> logger, IGraphClient graphClient)
		{
			_logger = logger;
			_graphClient = graphClient;
		}

		public async Task Setup()
		{
			_logger.LogDebug("Setting up Neo4j database");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<PipelinesRoot>("root")
					.Constrain(root => root.Id == PipelinesRoot.Identifier))
				.OnCreate()
				.Set("root", () => new PipelinesRoot())
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Neo4j database setup complete");
		}

		public async Task<IList<Pipeline>> CreateDefaults(IList<Pipeline> pipelines = null)
		{
			_logger.LogDebug("Creating default pipelines");

			pipelines ??= HardcodedDefaultPipelines.NewDefaultPipelines();

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			foreach (var newPipeline in pipelines)
			{
				await Add(newPipeline);
			}

			_logger.LogInformation("Created {PipelineCount} default pipelines", pipelines.Count);

			return pipelines;
		}

		public async Task Add(Pipeline newPipeline)
		{
			_logger.LogDebug("Adding pipeline {PipelineId}", newPipeline.Id);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<Pipeline>("pipeline")
					.Constrain(pipeline => pipeline.Id == newPipeline.Id))
				.OnCreate()
				.Set("pipeline", () => newPipeline)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<PipelinesRoot>("root").Constrain(r => r.Id == PipelinesRoot.Identifier))
				.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == newPipeline.Id))
				.Create("(root)-[r:EXISTS]->(pipeline)")
				.ExecuteWithoutResultsAsync();

			foreach (var node in newPipeline.Root)
			{
				await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Merge(path => path.Pattern<Node>("rootNode").Constrain(rootNode => rootNode.Id == node.Id))
					.Set("rootNode", () => node)
					.ExecuteWithoutResultsAsync();

				await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Node>("rootNode").Constrain(rootNode => rootNode.Id == node.Id))
					.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == newPipeline.Id))
					.Create("(pipeline)-[r:HAS_ROOT_NODE]->(rootNode)")
					.ExecuteWithoutResultsAsync();
			}

			_logger.LogInformation("Added pipeline {PipelineId} to database", newPipeline.Id);
		}

		public async Task<Pipeline> Get(Guid pipelineId)
		{
			_logger.LogDebug("Getting pipeline {PipelineId}", pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			throw new NotImplementedException();
		}

		public async Task<PipelineInfoDto> GetInfoDto(Guid pipelineId)
		{
			_logger.LogDebug("Getting pipeline {PipelineId} info", pipelineId);
			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var results = await _graphClient
				.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Pipeline>("pipeline")
					.Constrain(p => p.Id == pipelineId))
				.Return(pipeline => pipeline.As<PipelineInfoDto>())
				.ResultsAsync;

			var pipeline = results.FirstOrDefault();
			if (pipeline == default)
			{
				_logger.LogInformation("No pipeline for id {NotFoundId} found", pipelineId);

				return null;
			}

			_logger.LogInformation("Loaded pipeline info {PipelineId}", pipeline.Id);

			return pipeline;
		}

		public Task<IList<Pipeline>> Get()
		{
			throw new NotImplementedException();
		}

		public async Task<IList<PipelineInfoDto>> GetDtos()
		{
			_logger.LogDebug("Getting all pipeline dtos");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();


			var results = await _graphClient
				.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Pipeline>("pipeline"))
				.Return(pipeline => pipeline.As<PipelineInfoDto>())
				.ResultsAsync;

			var pipelineInfoDtos = results?.ToList() ?? new List<PipelineInfoDto>();

			_logger.LogInformation("Loaded {PipelineCount} pipeline dtos", pipelineInfoDtos.Count);
			return pipelineInfoDtos;
		}

		public Task<Pipeline> Update(Pipeline pipeline)
		{
			throw new NotImplementedException();
		}
	}
}
