using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using Newtonsoft.Json;
using PipelineService.Exceptions;
using PipelineService.Extensions;
using PipelineService.Models;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao.Impl
{
	public class InMemoryPipelinesExecutionDao : IPipelinesExecutionDao
	{
		private readonly ILogger<InMemoryPipelinesExecutionDao> _logger;
		private readonly IGraphClient _graphClient;
		private readonly IConfiguration _configuration;

		private static readonly IDictionary<Guid, PipelineExecutionRecord> Store =
			new ConcurrentDictionary<Guid, PipelineExecutionRecord>();

		public InMemoryPipelinesExecutionDao(ILogger<InMemoryPipelinesExecutionDao> logger,
			IGraphClient graphClient,
			IConfiguration configuration)
		{
			_logger = logger;
			_graphClient = graphClient;
			_configuration = configuration;
		}

		public async Task<PipelineExecutionRecord> Create(Guid pipelineId)
		{
			var executionRecord = new PipelineExecutionRecord
			{
				PipelineId = pipelineId,
				StartedOn = DateTime.UtcNow
			};

			_logger.LogInformation("Creating execution ({ExecutionId}) for pipeline {PipelineId}",
				executionRecord.Id, pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			var partitionRequest = _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match($"(n:{nameof(Operation)})")
				.Where("n.PipelineId=$pipeline_id").WithParam("pipeline_id", pipelineId)
				.AndWhere("NOT (n)-[:HAS_SUCCESSOR]->()")
				.With("collect(n) AS nodesList")
				.Call("hanse.markPartitions(nodesList, 'HAS_SUCCESSOR', $max_depth)").WithParam("max_depth",
					_configuration.GetValue("MaxSearchDepthPartitioning", 100))
				.Yield("maxLevel, visitedStamp")
				.Return(() => new
				{
					MaxLevel = Return.As<int>("maxLevel"),
					VisitedStamp = Return.As<string>("visitedStamp")
				});

			_logger.LogDebug("Cypher Request: {CypherRequest}", partitionRequest.Query.DebugQueryText);

			var partitionResult = (await partitionRequest.ResultsAsync).FirstOrDefault();
			if (partitionResult == null)
			{
				throw new NullReferenceException(
					"Pipeline database returned unexpected result for graph partitioning \nHint: procedure hanse.markPartitions(...) should be present in db");
			}

			_logger.LogDebug("Partitioned graph of pipeline {PipelineId} into {PartitionCount} partitions",
				pipelineId, partitionResult.MaxLevel);

			var executionRecordsRequest = _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match($"(n:{nameof(Operation)})")
				.Where("n._visited=$visited_stamp").WithParam("visited_stamp", partitionResult.VisitedStamp)
				.Return(() => new
				{
					ResultDatasets = Return.As<string>($"n.{nameof(Operation.OutputSerialized)}"),
					OperationId = Return.As<Guid>($"n.{nameof(Operation.Id)}"),
					PipelineId = Return.As<Guid>($"n.{nameof(Operation.PipelineId)}"),
					Level = Return.As<int>("n._level"),
					Name = Return.As<string>($"n.{nameof(Operation.OperationIdentifier)}"),
					HashAtEnqueuing = Return.As<string>($"n.{nameof(Operation.ComputedHash)}")
				})
				.OrderByDescending("n._level");

			var nodeExecutionRecords = (await executionRecordsRequest.ResultsAsync)
				.Select(r => new OperationExecutionRecord
				{
					ResultDatasets = r.ResultDatasets.StartsWith("{")
						? new List<Dataset> { JsonConvert.DeserializeObject<Dataset>(r.ResultDatasets) }
						: JsonConvert.DeserializeObject<IList<Dataset>>(r.ResultDatasets),
					OperationId = r.OperationId,
					PipelineId = r.PipelineId,
					Level = r.Level,
					Name = r.Name,
					HashAtEnqueuing = r.HashAtEnqueuing
				}).ToList();

			foreach (var nodeExecutionRecord in nodeExecutionRecords)
			{
				nodeExecutionRecord.Level = partitionResult.MaxLevel - nodeExecutionRecord.Level;
			}

			executionRecord.ToBeExecuted.AddAll(nodeExecutionRecords);

			Store.Add(executionRecord.Id, executionRecord);

			return executionRecord;
		}

		public Task<PipelineExecutionRecord> Get(Guid executionId)
		{
			if (!Store.TryGetValue(executionId, out var pipelineExecutionRecord))
			{
				throw new NotFoundException("No PipelineExecutionRecord with id found");
			}

			_logger.LogInformation("Loaded execution by id {ExecutionId}", executionId);
			return Task.FromResult(pipelineExecutionRecord);
		}

		public Task<PipelineExecutionRecord> Update(PipelineExecutionRecord execution)
		{
			if (Store.ContainsKey(execution.Id))
			{
				Store.Remove(execution.Id);
			}

			Store.Add(execution.Id, execution);

			_logger.LogInformation("Updated pipeline execution record {ExecutionId}", execution.Id);

			return Task.FromResult(execution);
		}

		public Task<PipelineExecutionRecord> GetLastExecutionForPipeline(Guid pipelineId)
		{
			var record = Store.Values.FirstOrDefault(exRec => exRec.PipelineId == pipelineId);

			if (record == default)
			{
				_logger.LogInformation("No execution record found for pipeline {PipelineId}", pipelineId);
				return Task.FromResult<PipelineExecutionRecord>(null);
			}

			_logger.LogInformation("Loaded last execution record {ExecutionId} for pipeline {PipelineId}",
				record.Id, pipelineId);

			return Task.FromResult(record);
		}
	}
}
