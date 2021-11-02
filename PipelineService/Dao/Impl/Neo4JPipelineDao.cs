using System;
using System.Collections.Generic;
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

			await CreatePipeline(newPipeline);

			foreach (var node in newPipeline.Root)
			{
				await CreateRootNode(newPipeline.Id, node);
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

		/// <summary>
		/// Creates a new pipeline in the store if it not already exists.
		/// </summary>
		/// <remarks>
		/// If a pipeline with the same id already exists, values will be merged.
		/// Ignores all <c>Node</c>s provided in this object (use <c>CreateRoot</c> for adding root nodes to a pipeline).
		/// </remarks>
		/// <param name="pipeline">An object with values that will be persisted.</param>
		public async Task<Pipeline> CreatePipeline(Pipeline pipeline)
		{
			_logger.LogDebug("Creating pipeline {PipelineId}", pipeline.Id);
			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var results = (await _graphClient.WithAnnotations<PipelineContext>().Tx.Cypher
				.Merge(path => path.Pattern<Pipeline>("ppln").Constrain(ppl => ppl.Id == pipeline.Id))
				.OnCreate()
				.Set("ppln", () => pipeline)
				.Return(ppln => ppln.As<Pipeline>())
				.ResultsAsync).ToArray();

			if (results.Length > 0)
			{
				_logger.LogInformation("Created pipeline {PipelineId}", pipeline.Id);
			}

			return results.FirstOrDefault();
		}

		public async Task CreateRootNode(Guid pipelineId, Node root)
		{
			_logger.LogDebug("Creating root node {NodeId} for pipeline {PipelineId}", root.Id, pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			// await _graphClient.WithAnnotations<PipelineContext>().Cypher
			// 	.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == pipelineId))
			// 	.Create(path => path.Pattern((Pipeline pipeline) => pipeline.Root, "root")
			// 		.Prop(null, () => root))
			// 	.ExecuteWithoutResultsAsync();

			// TODO: Merge this into a single db call using annotations
			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<Node>("rootNode").Constrain(rootNode => rootNode.Id == root.Id))
				.OnCreate()
				.Set("rootNode", () => root)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Node>("rootNode").Constrain(rootNode => rootNode.Id == root.Id))
				.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == pipelineId))
				.Create("(pipeline)-[r:HAS_ROOT_NODE]->(rootNode)")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Created root node {NodeId} for pipeline {PipelineId}", root.Id, pipelineId);
		}
	}
}
