using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using PipelineService.Exceptions;
using PipelineService.Models;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao.Impl
{
	public class InMemoryPipelinesExecutionDao : IPipelinesExecutionDao
	{
		private readonly ILogger<InMemoryPipelinesExecutionDao> _logger;
		private readonly IGraphClient _graphClient;

		private static readonly IDictionary<Guid, PipelineExecutionRecord> Store =
			new ConcurrentDictionary<Guid, PipelineExecutionRecord>();

		public InMemoryPipelinesExecutionDao(ILogger<InMemoryPipelinesExecutionDao> logger, IGraphClient graphClient)
		{
			_logger = logger;
			_graphClient = graphClient;
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

			var request = _graphClient.WithAnnotations<PipelineContext>().Cypher
				.Match("(n:Node)")
				.Where("n.PipelineId=$pipeline_id").WithParam("pipeline_id", pipelineId)
				.AndWhere("NOT (n)-[:HAS_SUCCESSOR]->()")
				.With("collect(n) AS nodesList")
				.Call("hanse.markPartitions(nodesList, 'HAS_SUCCESSOR', 25)")
				.Yield("maxLevel, visitedStamp")
				.Return(() => new
				{
					MaxLevel = Return.As<int>("maxLevel"),
					VisitedStamp = Return.As<string>("visitedStamp")
				});

			_logger.LogDebug("Cypher Request: {CypherRequest}", request.Query.DebugQueryText);

			var partitionResult = (await request.ResultsAsync).FirstOrDefault();
			if (partitionResult == null)
			{
				throw new NullReferenceException(
					"Pipeline database returned unexpected result for graph partitioning \nHint: procedure hanse.markPartitions(...) should be present in db");
			}

			_logger.LogDebug("Partitioned graph of pipeline {PipelineId} into {PartitionCount} partitions",
				pipelineId, partitionResult.MaxLevel);


			// TODO query predecessors

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
