using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using Neo4jClient.Extensions;
using Newtonsoft.Json;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
	public class Neo4JPipelinesDao : IPipelinesDao
	{
		private readonly ILogger<Neo4JPipelinesDao> _logger;
		private readonly IGraphClient _graphClient;

		public Neo4JPipelinesDao(ILogger<Neo4JPipelinesDao> logger, IGraphClient graphClient)
		{
			_logger = logger;
			_graphClient = graphClient;
		}

		public async Task Setup()
		{
			_logger.LogDebug("Setting up Neo4j database");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			_graphClient.WithAnnotations<PipelineContext>().Cypher.CreateUniqueConstraint<Operation>(n => n.Id);
			_graphClient.WithAnnotations<PipelineContext>().Cypher.CreateUniqueConstraint<Pipeline>(n => n.Id);

			_logger.LogInformation("Neo4j database setup complete");
		}

		public async Task<IList<Pipeline>> CreatePipelines(IList<Pipeline> pipelines)
		{
			_logger.LogDebug("Creating {PipelineCount} pipelines", pipelines.Count);

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

			foreach (var rootOperation in newPipeline.Root)
			{
				await CreateRootOperationGetType(newPipeline.Id, rootOperation);
				await CreateSuccessorsGetType(rootOperation);
			}

			_logger.LogInformation("Added pipeline {PipelineId} to database", newPipeline.Id);
		}

		/// <summary>
		/// Helper method for recursively creating all successor operations for an operation.
		/// </summary>
		private async Task CreateSuccessorsGetType<TP>(TP operation) where TP : Operation
		{
			foreach (var successor in operation.Successors)
			{
				await CreateSuccessorGetType(operation.Id, successor);
				await CreateSuccessorsGetType(successor);
			}
		}

		/// <summary>
		/// Helper for creating defaults.
		/// </summary>
		private async Task CreateSuccessorGetType(Guid predecessorId, Operation successor)
		{
			if (successor.GetType() == typeof(Operation))
				await CreateSuccessor(new List<Guid> { predecessorId }, successor);
			else throw new InvalidOperationException($"Type {nameof(Operation)} not supported");
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

		public async Task<IList<PipelineInfoDto>> GetDtos(string userIdentifier = default)
		{
			_logger.LogDebug("Getting all pipeline dtos");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient
				.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Pipeline>("pipeline"));

			if (userIdentifier != default)
			{
				query = query.Where("pipeline.UserIdentifier=$user_identifier").WithParam("user_identifier", userIdentifier);
			}

			var results = await query.Return(pipeline => pipeline.As<PipelineInfoDto>()).ResultsAsync;

			var pipelineInfoDtos = results?.ToList() ?? new List<PipelineInfoDto>();

			_logger.LogInformation("Loaded {PipelineCount} pipeline dtos", pipelineInfoDtos.Count);
			return pipelineInfoDtos;
		}

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
		private async Task CreateRootOperationGetType(Guid pipelineId, Operation operation)
		{
			if (operation.GetType() == typeof(Operation))
				await CreateRootOperation(pipelineId, operation);
			else throw new InvalidOperationException($"Type {nameof(Operation)} not supported");
		}

		public async Task CreateRootOperation<TN>(Guid pipelineId, TN root) where TN : Operation
		{
			_logger.LogDebug("Creating root operation {OperationId} for pipeline {PipelineId}", root.Id, pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			root.ChangedOn = DateTime.UtcNow;

			// TODO: Merge this into a single db call using annotations
			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<TN>("operationNode").Constrain(operationNode => operationNode.Id == root.Id))
				.OnCreate()
				.Set("operationNode", () => root)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<TN>("rootoperation").Constrain(rootoperation => rootoperation.Id == root.Id))
				.Match(path => path.Pattern<Pipeline>("pipeline").Constrain(pipeline => pipeline.Id == pipelineId))
				.Merge("(pipeline)-[r:HAS_ROOT_NODE]->(rootoperation)")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Created root operation {OperationId} for pipeline {PipelineId}", root.Id, pipelineId);
		}

		public async Task CreateSuccessor<T>(IList<Guid> predecessorIds, T successor) where T : Operation
		{
			_logger.LogDebug("Making {SuccessorOperationId} successor of {@PredecessorOperationIds}",
				successor.Id, predecessorIds);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			successor.ChangedOn = DateTime.UtcNow;

			// TODO: Merge this into a single db call using annotations
			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<T>("operationNode").Constrain(operationNode => operationNode.Id == successor.Id))
				.OnCreate()
				.Set("operationNode", () => successor)
				.ExecuteWithoutResultsAsync();

			foreach (var predecessorId in predecessorIds)
			{
				await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Operation>("predNode").Constrain(predNode => predNode.Id == predecessorId))
					.Match(path => path.Pattern<Operation>("sucNode").Constrain(sucNode => sucNode.Id == successor.Id))
					.Merge("(predNode)-[r:HAS_SUCCESSOR]->(sucNode)")
					.ExecuteWithoutResultsAsync();
			}

			_logger.LogInformation("Made {SuccessorOperationId} successor of {@PredecessorOperationId}",
				successor.Id, predecessorIds);
		}

		public async Task<Operation> GetOperation(Guid operationId)
		{
			_logger.LogDebug("Loading operation {OperationId}", operationId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var operation = await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Operation>("operationNode")
					.Constrain(operationNode => operationNode.Id == operationId))
				.Return<Operation>("operationNode")
				.ResultsAsync;

			_logger.LogInformation("Loaded operation {OperationId}", operationId);

			return operation.FirstOrDefault();
		}

		public async Task UpdateOperation<T>(T operation) where T : Operation
		{
			_logger.LogDebug("Updating operation {OperationId}", operation.Id);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();
			operation.ChangedOn = DateTime.UtcNow;

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Merge(path => path.Pattern<T>("n").Constrain(n => n.Id == operation.Id))
				.Set("n", () => operation)
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Updated operation {OperationId}", operation.Id);
		}

		public async Task DeleteOperation(Guid operationId)
		{
			_logger.LogDebug("Deleting node {OperationId}", operationId);

			await _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match(path => path.Pattern<Operation>("operationNode")
					.Constrain(operationNode => operationNode.Id == operationId))
				.DetachDelete("operationNode")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Deleted operation {OperationId}", operationId);
		}

		public async Task<IList<OperationTupleSingleInput>> GetTuplesSingleInput()
		{
			_logger.LogDebug("Loading all tuples of single input operations");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var results = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Operation, Operation>("predecessor", "successor"))
					.Return(() => new
					{
						predecessor = Return.As<Operation>("operationNode"),
						successor = Return.As<Operation>("successor"),
					})
					.ResultsAsync)
				.Select(tuple => new OperationTupleSingleInput
				{
					Description = $"{tuple.predecessor.OperationIdentifier} -> {tuple.successor.OperationIdentifier}",
					NodeId = tuple.predecessor.Id,
					OperationId = tuple.predecessor.OperationId,
					OperationIdentifier = tuple.predecessor.OperationIdentifier,
					OperationConfiguration = tuple.predecessor.OperationConfiguration,
					OperationInputs = tuple.predecessor.Inputs,
					OperationOutput = tuple.successor.Output,
					TargetNodeId = tuple.successor.Id,
					TargetOperation = tuple.successor.OperationIdentifier,
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

			var resultNodes = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Operation>("operationNode"))
					.Where((Operation operationNode) => operationNode.PipelineId == pipelineId)
					.Return(() => new
					{
						Id = Return.As<Guid>($"operationNode.{nameof(Operation.Id)}"),
						Label = Return.As<string>($"operationNode.{nameof(Operation.OperationIdentifier)}"),
						InputsSerialized = Return.As<string>($"operationNode.{nameof(Operation.InputsSerialized)}"),
						OutputSerialized = Return.As<string>($"operationNode.{nameof(Operation.OutputSerialized)}"),
					})
					.ResultsAsync)
				.ToList()
				.Select(o => new VisualizationOperationDto
				{
					Id = o.Id,
					Label = o.Label,
					Inputs = JsonConvert.DeserializeObject<IList<Dataset>>(o.InputsSerialized),
					Output = JsonConvert.DeserializeObject<Dataset>(o.OutputSerialized)
				});

			foreach (var resultNode in resultNodes)
			{
				dto.Nodes.Add(resultNode);
			}

			var resultEdges = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
					.Match(path => path.Pattern<Operation, Operation>("operationNode", "successor"))
					.Where((Operation operationNode, Operation successor) =>
						operationNode.PipelineId == pipelineId && successor.PipelineId == pipelineId)
					.Return(() => new
					{
						NodeId = Return.As<Guid>("operationNode.Id"),
						SuccessorId = Return.As<Guid>("successor.Id"),
					})
					.ResultsAsync
				).ToArray();

			foreach (var edge in resultEdges)
			{
				dto.Edges.Add(new VisEdge { Id = Guid.NewGuid(), From = edge.NodeId, To = edge.SuccessorId });
			}

			_logger.LogInformation("Loaded pipeline {PipelineId} for visualization", pipelineId);
			return dto;
		}

		public async Task<IList<Dataset>> GetOutputDatasets(IList<Guid> operationIds)
		{
			_logger.LogDebug("Loading output datasets for operations {@OperationIds}", operationIds);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var datasets = new List<Dataset>();

			// TODO: This is a very inefficient way of doing this, but it keeps the order of datasets the same as the order of operationIds.
			foreach (var operationId in operationIds)
			{
				var dataset = (await _graphClient.WithAnnotations<PipelineContext>().Cypher
						.Match(path => path.Pattern<Operation>("o"))
						.Where((Operation o) => o.Id == operationId)
						.Return(() => new { OutputSerialized = Return.As<string>("o.OutputSerialized") })
						.ResultsAsync)
					.Select(o => JsonConvert.DeserializeObject<Dataset>(o.OutputSerialized))
					.SingleOrDefault();
				datasets.Add(dataset);
			}

			_logger.LogInformation("Loaded {DatasetCount} output datasets for operations {@OperationIds}",
				datasets.Count, operationIds);

			return datasets;
		}
	}
}
