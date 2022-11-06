using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using Newtonsoft.Json;
using PipelineService.Extensions;
using PipelineService.Helper;
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

			_graphClient.WithAnnotations<PipelineGraphContext>().Cypher.CreateUniqueConstraint<Operation>(n => n.Id);
			_graphClient.WithAnnotations<PipelineGraphContext>().Cypher.CreateUniqueConstraint<Pipeline>(n => n.Id);

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
				.WithAnnotations<PipelineGraphContext>().Cypher
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

		public async Task<bool> DeletePipeline(Guid pipelineId)
		{
			_logger.LogDebug("Deleting pipeline {PipelineId}", pipelineId);
			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();
			try
			{
				await _graphClient
					.WithAnnotations<PipelineGraphContext>().Cypher
					.Match(path => path.Pattern<Pipeline>("pipeline")
						.Constrain(p => p.Id == pipelineId))
					.Match(path => path.Pattern<Operation>("operation")
						.Constrain(o => o.PipelineId == pipelineId))
					.DetachDelete("pipeline, operation")
					.ExecuteWithoutResultsAsync();
			}
			catch (Exception e)
			{
				_logger.LogWarning(e, "Failed to delete pipeline {PipelineId}", pipelineId);
				return false;
			}

			return true;
		}

		public async Task<PipelineInfoDto> UpdatePipeline(PipelineInfoDto pipelineDto)
		{
			_logger.LogDebug("Updating pipeline {PipelineId}", pipelineDto.Id);
			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Merge(path => path.Pattern<Pipeline>("p").Constrain(n => n.Id == pipelineDto.Id))
				.Set("p", () => pipelineDto)
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Updated pipeline {PipelineId}", pipelineDto.Id);
			return await GetInfoDto(pipelineDto.Id);
		}

		public async Task<PaginatedList<PipelineInfoDto>> GetDtos(Pagination pagination = null,
			string userIdentifier = default)
		{
			_logger.LogDebug("Getting pipeline dtos...");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient
				.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Pipeline>("pipeline"));

			if (userIdentifier != default)
			{
				query = query.Where("pipeline.UserIdentifier=$user_identifier").WithParam("user_identifier", userIdentifier);
			}

			var typedQuery = query.Return(pipeline => pipeline.As<PipelineInfoDto>());
			if (pagination != null)
			{
				if (string.IsNullOrEmpty(pagination.Sort))
				{
					pagination.Sort = nameof(Pipeline.CreatedOn);
				}

				// TODO: parameterize this, this is a potential injection point
				typedQuery = pagination.Order == "asc"
					? typedQuery.OrderBy($"pipeline.{pagination.Sort}")
					: typedQuery.OrderByDescending($"pipeline.{pagination.Sort}");
				typedQuery = typedQuery
					.Skip(pagination.Page * pagination.PageSize)
					.Limit(pagination.PageSize);
			}

			var results = await typedQuery.ResultsAsync;

			var pipelineInfoDtos = results?.ToList() ?? new List<PipelineInfoDto>();
			var pipelineCount = (await query
				.Return(() => Return.As<int>("count(pipeline)"))
				.ResultsAsync).FirstOrDefault();

			_logger.LogInformation("Loaded {PipelineCount} pipeline dtos", pipelineInfoDtos.Count);
			return new PaginatedList<PipelineInfoDto>()
			{
				Items = pipelineInfoDtos,
				Page = pagination?.Page ?? 0,
				PageSize = pagination?.PageSize ?? pipelineCount,
				TotalItems = pipelineCount
			};
		}

		public async Task CreatePipeline(Pipeline pipeline)
		{
			_logger.LogDebug("Creating pipeline {PipelineId}", pipeline.Id);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var results = (await _graphClient.WithAnnotations<PipelineGraphContext>().Tx.Cypher
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
			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Merge(path => path.Pattern<TN>("operationNode").Constrain(operationNode => operationNode.Id == root.Id))
				.OnCreate()
				.Set("operationNode", () => root)
				.ExecuteWithoutResultsAsync();

			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
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
			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Merge(path => path.Pattern<T>("operationNode").Constrain(operationNode => operationNode.Id == successor.Id))
				.OnCreate()
				.Set("operationNode", () => successor)
				.ExecuteWithoutResultsAsync();

			foreach (var predecessorId in predecessorIds)
			{
				await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
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

			var operation = await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Operation>("operationNode")
					.Constrain(operationNode => operationNode.Id == operationId))
				.Return<Operation>("operationNode")
				.ResultsAsync;

			_logger.LogInformation("Loaded operation {OperationId}", operationId);

			return operation.FirstOrDefault();
		}

		public async Task<IList<Operation>> GetOperations(Guid pipelineId)
		{
			_logger.LogDebug("Loading all operations for pipeline {PipelineId}", pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var operations = (await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Operation>("operation")
					.Constrain(operation => operation.PipelineId == pipelineId))
				.Return<Operation>("operation")
				.ResultsAsync).ToList();

			_logger.LogInformation("Loaded {OperationCount} operations for pipeline {PipelineId}", operations.Count,
				pipelineId);

			return operations;
		}

		public async Task UpdateOperation<T>(T operation) where T : Operation
		{
			_logger.LogDebug("Updating operation {OperationId}", operation.Id);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();
			operation.ChangedOn = DateTime.UtcNow;

			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Merge(path => path.Pattern<T>("n").Constrain(n => n.Id == operation.Id))
				.Set("n", () => operation)
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Updated operation {OperationId}", operation.Id);
		}

		public async Task DeleteOperation(Guid operationId)
		{
			_logger.LogDebug("Deleting node {OperationId}", operationId);

			await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Operation>("operationNode")
					.Constrain(operationNode => operationNode.Id == operationId))
				.DetachDelete("operationNode")
				.ExecuteWithoutResultsAsync();

			_logger.LogInformation("Deleted operation {OperationId}", operationId);
		}

		public async Task<IList<OperationTuples>> GetOperationTuples()
		{
			_logger.LogDebug("Loading all tuples of single input operations");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Pipeline>("pipeline"))
				.Where($"pipeline.{nameof(Pipeline.LastRunSuccess)} IS NOT NULL")
				.Call("{WITH pipeline " +
				      "MATCH (predecessor:Operation)-[:HAS_SUCCESSOR]->(target:Operation) " +
				      "WHERE predecessor.PipelineId = pipeline.Id " +
				      "RETURN predecessor AS predecessor, target AS target, apoc.node.degree(predecessor, \"<HAS_SUCCESSOR\") AS predecessorInDegree } ")
				.Return(() => new
				{
					predecessor = Return.As<Operation>("predecessor"),
					target = Return.As<Operation>("target"),
					predecessorInDegree = Return.As<int>("predecessorInDegree")
				});

			_logger.LogDebug("Exporting with query {ExportQuery}", query.Query.QueryText);

			var results = (await query.ResultsAsync)
				.Select(tuple => new OperationTuples
				{
					TupleDescription = $"{tuple.predecessor.OperationIdentifier} -> {tuple.target.OperationIdentifier}",
					PredecessorOperationIdentifier = $"{tuple.predecessor.OperationId}-{tuple.predecessor.OperationIdentifier}",
					PredecessorOperationConfiguration = tuple.predecessor.OperationConfiguration,
					PredecessorOperationInputs = tuple.predecessor.Inputs,
					PredecessorOperationOutput = tuple.predecessor.Outputs,
					PredecessorInDegree = tuple.predecessorInDegree,
					TargetOperationIdentifier =
						OperationHelper.GetGlobalUniqueOperationIdentifier(tuple.target.OperationId,
							tuple.target.OperationIdentifier),
					TargetInputs = tuple.target.Inputs,
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

			var resultNodes = (await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
					.Match(path => path.Pattern<Operation>("operationNode"))
					.Where((Operation operationNode) => operationNode.PipelineId == pipelineId)
					.Return(() => new
					{
						Id = Return.As<Guid>($"operationNode.{nameof(Operation.Id)}"),
						OperationIdentifier = Return.As<string>($"operationNode.{nameof(Operation.OperationIdentifier)}"),
						OperationId = Return.As<string>($"operationNode.{nameof(Operation.OperationId)}"),
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
					Outputs = JsonConvert.DeserializeObject<IList<Dataset>>(o.OutputSerialized.StartsWith("{")
						? $"[{o.OutputSerialized}]"
						: o.OutputSerialized),
					OperationIdentifier = $"{o.OperationId}-{o.OperationIdentifier}", // TODO centralize this see tuple generation
					OperationId = o.Id,
					OperationTemplateId = Guid.Parse(o.OperationId),
					OperationName = o.OperationIdentifier
				});

			foreach (var resultNode in resultNodes)
			{
				resultNode.Title = string.Join(" | ", resultNode.Outputs.Select(o => o.Type.ToString()));
				dto.Nodes.Add(resultNode);
			}

			var resultEdges = (await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
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

		public async Task<IList<Dataset>> GetOutputDatasets(Guid operationId)
		{
			_logger.LogDebug("Loading output datasets for operation {OperationId}", operationId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var datasets = new List<Dataset>();

			var dataset = (await _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
					.Match(path => path.Pattern<Operation>("o"))
					.Where((Operation o) => o.Id == operationId)
					.Return(() => new { OutputSerialized = Return.As<string>("o.OutputSerialized") })
					.ResultsAsync)
				.Select(o =>
					JsonConvert.DeserializeObject<IList<Dataset>>(o.OutputSerialized.StartsWith("{")
						? $"[{o.OutputSerialized}]"
						: o.OutputSerialized))
				.SingleOrDefault();
			datasets.AddAll(dataset);


			_logger.LogInformation("Loaded {DatasetCount} output datasets for operations {OperationId}",
				datasets.Count, operationId);

			return datasets;
		}

		public async Task<PipelineExport> ExportPipeline(Guid pipelineId)
		{
			_logger.LogDebug("Exporting pipeline {PipelineId}", pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();
			var query = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(op => op.Pattern("op"))
				.Where((Operation op) => op.PipelineId == pipelineId)
				.Match("(op)-[rl]->()")
				.Match(ppl => ppl.Pattern("ppl"))
				.Where((Pipeline ppl) => ppl.Id == pipelineId)
				.Match("(ppl)-[prl]->()")
				.With("collect(rl) as rls, collect(prl) as prls")
				.Call("apoc.export.json.data([], rls , null, {stream: true, writeNodeProperties: true})")
				.Yield("data as data_ops")
				.Call("apoc.export.json.data([], prls , null, {stream: true, writeNodeProperties: true})")
				.Yield("data as data_ps")
				.Return(() => new PipelineExport
				{
					OperationData = Return.As<string>("data_ops"),
					PipelineData = Return.As<string>("data_ps")
				});

			_logger.LogDebug("Exporting with query {ExportQuery}", query.Query.QueryText);

			var results = (await query.ResultsAsync).FirstOrDefault();
			if (results == default)
			{
				_logger.LogInformation("Failed to export pipeline {PipelineId}", pipelineId);
				return null;
			}

			results.CreatedOn = DateTime.UtcNow;
			results.PipelineId = pipelineId;

			_logger.LogInformation("Exported pipeline {PipelineId}", pipelineId);

			return results;
		}

		public async Task<IEnumerable<string>> GetUsedOperationIdentifiers(bool unique = false)
		{
			_logger.LogDebug("Loading operation ids from all available pipelines...");

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Operation>("o"))
				.Return(() => new
				{
					OperationId = Return.As<Guid>($"o.{nameof(Operation.OperationId)}"),
					OperationIdentifier = Return.As<string>($"o.{nameof(Operation.OperationIdentifier)}")
				});

			var ids = (await query.ResultsAsync)
				.Select(o => OperationHelper.GetGlobalUniqueOperationIdentifier(o.OperationId, o.OperationIdentifier))
				.ToList();

			if (unique)
			{
				_logger.LogInformation("Loaded unique operation ids from all stored pipelines");
				return ids.ToHashSet();
			}

			_logger.LogInformation("Loaded all operation ids from all pipelines");
			return ids;
		}

		public async Task<int> GetOperationCount(Guid pipelineId)
		{
			_logger.LogDebug("Loading operation count for pipeline {PipelineId}", pipelineId);
			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match(path => path.Pattern<Operation>("o"))
				.Where((Operation o) => o.PipelineId == pipelineId)
				.Return(() => Return.As<int>("count(o)"));

			return (await query.ResultsAsync).FirstOrDefault();
		}

		public async Task<IList<string>> GetPredecessorHashes(Guid operationId)
		{
			_logger.LogDebug("Loading predecessor hashes for operation {OperationId}", operationId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var query = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match($"(p:{nameof(Operation)})-[:HAS_SUCCESSOR] -> (o:{nameof(Operation)})")
				.Where((Operation o) => o.Id == operationId)
				.Return(() => Return.As<string>($"p.{nameof(Operation.OperationHash)}"));

			var result = (await query.ResultsAsync).ToList();

			_logger.LogInformation("Loaded {PredecessorHashCount} predecessor hashes for operation {OperationId}",
				result.Count, operationId);

			return result;
		}
	}
}
