using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;
using Node = PipelineService.Models.Pipeline.Node;

namespace PipelineService.Dao.Impl
{
	public class Neo4JPipelineDao
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

			_graphClient.WithAnnotations<PipelineContext>().Cypher.CreateUniqueConstraint<Node>(n => n.Id);
			_graphClient.WithAnnotations<PipelineContext>().Cypher.CreateUniqueConstraint<Pipeline>(n => n.Id);

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

		private async Task Add(Pipeline newPipeline)
		{
			_logger.LogDebug("Adding pipeline {PipelineId}", newPipeline.Id);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			await CreatePipeline(newPipeline);

			foreach (var rootNode in newPipeline.Root)
			{
				await CreateRootNodeGetType(newPipeline.Id, rootNode);
				await CreateSuccessorsGetType(rootNode, rootNode.Successors);
			}

			_logger.LogInformation("Added pipeline {PipelineId} to database", newPipeline.Id);
		}

		/// <summary>
		/// Helper method for creating successors for default pipelines.
		/// </summary>
		private async Task CreateSuccessorsGetType<TP>(TP node, IEnumerable<Node> successors) where TP : Node
		{
			foreach (var successor in successors)
			{
				await CreateSuccessorGetType<TP>(node.Id, successor);
				await CreateSuccessorsGetType(successor, successor.Successors);
			}
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

		/// <summary>
		/// Creates a new pipeline in the store if it not already exists.
		/// </summary>
		/// <remarks>
		/// If a pipeline with the same id already exists, values will be merged.
		/// Ignores all <c>Node</c>s provided in this object (use <c>CreateRoot</c> for adding root nodes to a pipeline).
		/// </remarks>
		/// <param name="pipeline">An object with values that will be persisted.</param>
		public async Task CreatePipeline(Pipeline pipeline)
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
		}

		/// <summary>
		/// Helper for creating defaults.
		/// </summary>
		private async Task CreateRootNodeGetType(Guid pipelineId, Node node)
		{
			if (node.GetType() == typeof(NodeSingleInput))
				await CreateRootNode(pipelineId, (NodeSingleInput)node);
			else if (node.GetType() == typeof(NodeDoubleInput))
				await CreateRootNode(pipelineId, (NodeDoubleInput)node);
			else if (node.GetType() == typeof(NodeFileInput))
				await CreateRootNode(pipelineId, (NodeFileInput)node);
			else throw new InvalidOperationException($"Type {nameof(Node)} not supported");
		}

		public async Task CreateRootNode<TN>(Guid pipelineId, TN root) where TN : Node
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
				.Merge(path => path.Pattern<TN>("node").Constrain(node => node.Id == root.Id))
				.OnCreate()
				.Set("node", () => root)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<TN>("rootNode").Constrain(rootNode => rootNode.Id == root.Id))
				.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == pipelineId))
				.Merge("(pipeline)-[r:HAS_ROOT_NODE]->(rootNode)")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Created root node {NodeId} for pipeline {PipelineId}", root.Id, pipelineId);
		}

		/// <summary>
		/// Helper for creating defaults.
		/// </summary>
		private async Task CreateSuccessorGetType<TP>(Guid predecessorId, Node successor) where TP : Node
		{
			if (successor.GetType() == typeof(NodeSingleInput))
				await CreateSuccessor<TP, NodeSingleInput>(predecessorId, (NodeSingleInput)successor);
			else if (successor.GetType() == typeof(NodeDoubleInput))
				await CreateSuccessor<TP, NodeDoubleInput>(predecessorId, (NodeDoubleInput)successor);
			else if (successor.GetType() == typeof(NodeFileInput))
				await CreateSuccessor<TP, NodeFileInput>(predecessorId, (NodeFileInput)successor);
			else throw new InvalidOperationException($"Type {nameof(Node)} not supported");
		}

		public async Task CreateSuccessor<TP, TS>(Guid predecessorId, TS successor) where TP : Node where TS : Node
		{
			_logger.LogDebug("Making {SuccessorNodeId} successor of {PredecessorNodeId}", successor.Id, predecessorId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			// await _graphClient.WithAnnotations<PipelineContext>().Cypher
			// 	.Match(
			// 		path => path.Pattern<Node>("predNode").Constrain(predNode => predNode.Id == predecessorId),
			// 		path2 => path2.Pattern<Node>("sucNode").Constrain(sucNode => sucNode.Id == successor.Id))
			// 	.Create(path => path.Pattern((Node predNode) => predNode.Successors, "sucNode"))
			// 	.ExecuteWithoutResultsAsync();

			// TODO: Merge this into a single db call using annotations
			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<TS>("node").Constrain(node => node.Id == successor.Id))
				.OnCreate()
				.Set("node", () => successor)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<TP>("predNode").Constrain(predNode => predNode.Id == predecessorId))
				.Match(path => path.Pattern<TS>("sucNode").Constrain(sucNode => sucNode.Id == successor.Id))
				.Merge("(predNode)-[r:HAS_SUCCESSOR]->(sucNode)")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Made {SuccessorNodeId} successor of {PredecessorNodeId}", successor.Id, predecessorId);
		}

		public async Task<Node> GetNode(Guid nodeId)
		{
			_logger.LogDebug("Loading node {NodeId} (autoresolving type)", nodeId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var labels = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
				             .Match(path => path.Pattern<Node>("n").Constrain(node => node.Id == nodeId))
				             .Return(() => Return.As<List<string>>("distinct labels(n)")).ResultsAsync).FirstOrDefault() ??
			             new List<string>();

			if (labels.Contains(nameof(NodeFileInput)))
			{
				return await GetNode<NodeFileInput>(nodeId);
			}

			if (labels.Contains(nameof(NodeSingleInput)))
			{
				return await GetNode<NodeSingleInput>(nodeId);
			}

			if (labels.Contains(nameof(NodeDoubleInput)))
			{
				return await GetNode<NodeDoubleInput>(nodeId);
			}

			throw new InvalidOperationException($"Autoresolving type {nameof(Node)} not supported");
		}

		public async Task<T> GetNode<T>(Guid nodeId) where T : Node
		{
			_logger.LogDebug("Loading node {NodeId}", nodeId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var node = await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<T>("node").Constrain(node => node.Id == nodeId))
				.Return<T>("node")
				.ResultsAsync;

			_logger.LogInformation("Loaded node {NodeId}", nodeId);

			return node.FirstOrDefault();
		}

		public async Task<IList<NodeTupleSingleInput>> GetTuplesSingleInput()
		{
			_logger.LogDebug("Loading all tuples of single input operations");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var results = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<NodeSingleInput, NodeSingleInput>("node", "successor"))
					.Return(() => new
					{
						node = Return.As<NodeSingleInput>("node"),
						successor = Return.As<NodeSingleInput>("successor"),
					})
					.ResultsAsync)
				.Select(tuple => new NodeTupleSingleInput
				{
					Description = $"{tuple.node.Operation} -> {tuple.successor.Operation}",
					NodeId = tuple.node.Id,
					DatasetHash = tuple.node.InputDatasetHash,
					OperationId = tuple.node.OperationId,
					Operation = tuple.node.Operation,
					OperationConfiguration = tuple.node.OperationConfiguration,
					TargetNodeId = tuple.successor.Id,
					TargetOperation = tuple.successor.Operation,
					TargetOperationId = tuple.successor.OperationId
				})
				.ToList();

			_logger.LogInformation("Loaded {TupleCount} tuples of single input operations", results.Count);

			return results;
		}

		public async Task<PipelineVisualizationDto> GetVisDto(Guid pipelineId)
		{
			_logger.LogDebug("Loading pipeline {PipelineId} for visualization", pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var infoDto = await GetInfoDto(pipelineId);
			if (infoDto == null)
			{
				return null;
			}

			var dto = new PipelineVisualizationDto
			{
				PipelineId = infoDto.Id,
				PipelineName = infoDto.Name
			};

			var results = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Node, Node>("node", "successor"))
					.Where((Node node, Node successor) => node.PipelineId == pipelineId && successor.PipelineId == pipelineId)
					.Return(() => new
					{
						NodeId = Return.As<Guid>("node.Id"),
						NodeOperation = Return.As<string>("node.Operation"),
						SuccessorId = Return.As<Guid>("successor.Id"),
						SuccessorOperation = Return.As<string>("successor.Operation")
					})
					.ResultsAsync
				).ToArray();

			foreach (var result in results)
			{
				if (dto.Nodes.All(n => n.Id != result.NodeId))
					dto.Nodes.Add(new VisNode { Id = result.NodeId, Label = result.NodeOperation });
				if (dto.Nodes.All(n => n.Id != result.SuccessorId))
					dto.Nodes.Add(new VisNode { Id = result.SuccessorId, Label = result.SuccessorOperation });
				dto.Edges.Add(new VisEdge { Id = Guid.NewGuid(), From = result.NodeId, To = result.SuccessorId });
			}

			_logger.LogInformation("Loaded pipeline {PipelineId} for visualization", pipelineId);
			return dto;
		}
	}
}
