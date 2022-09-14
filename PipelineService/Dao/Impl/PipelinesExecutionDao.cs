using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Neo4jClient;
using Neo4jClient.Cypher;
using Neo4jClient.DataAnnotations;
using PipelineService.Exceptions;
using PipelineService.Models;
using PipelineService.Models.Enums;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Dao.Impl
{
	public class PipelinesExecutionDao : IPipelinesExecutionDao
	{
		private readonly ILogger<PipelinesExecutionDao> _logger;
		private readonly IGraphClient _graphClient;
		private readonly IConfiguration _configuration;
		private readonly EfDatabaseContext _context;

		public PipelinesExecutionDao(
			ILogger<PipelinesExecutionDao> logger,
			IConfiguration configuration,
			IGraphClient graphClient,
			EfDatabaseContext context)
		{
			_logger = logger;
			_configuration = configuration;
			_graphClient = graphClient;
			_context = context;
		}

		public async Task<PipelineExecutionRecord> Create(Guid pipelineId,
			ExecutionStrategy strategy = ExecutionStrategy.Lazy)
		{
			var executionRecord = new PipelineExecutionRecord
			{
				PipelineId = pipelineId,
				StartedOn = DateTime.UtcNow
			};

			_logger.LogDebug("Creating execution ({ExecutionId}) for pipeline {PipelineId}",
				executionRecord.Id, pipelineId);

			if (!_graphClient.IsConnected) await _graphClient.ConnectAsync();

			_logger.LogInformation(
				"Creating execution plan for pipeline {PipelineId} according to {ExecutionStrategy} strategy...",
				pipelineId, strategy);

			var partitionRequest = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match($"(n:{nameof(Operation)})")
				.Where("n.PipelineId=$pipeline_id").WithParam("pipeline_id", pipelineId)
				.AndWhere(strategy == ExecutionStrategy.Lazy ? "NOT (n)-[:HAS_SUCCESSOR]->()" : "NOT ()-[:HAS_SUCCESSOR]->(n)")
				.With("collect(n) AS nodesList")
				.Call(
					$"hanse.partition.{(strategy == ExecutionStrategy.Lazy ? "lazy" : "eager")}(nodesList, 'HAS_SUCCESSOR', $max_depth)")
				.WithParam("max_depth",
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
					"Pipeline database returned unexpected result for graph partitioning \nHint: procedure hanse.partition.lazy(...) should be present in db");
			}

			_logger.LogDebug("Partitioned graph of pipeline {PipelineId} into {PartitionCount} partitions",
				pipelineId, partitionResult.MaxLevel);

			var executionRecordsRequest = _graphClient.WithAnnotations<PipelineGraphContext>().Cypher
				.Match($"(n:{nameof(Operation)})")
				.Where("n._visited=$visited_stamp").WithParam("visited_stamp", partitionResult.VisitedStamp)
				.Return(() => new
				{
					ResultDatasets = Return.As<string>($"n.{nameof(Operation.OutputSerialized)}"),
					OperationId = Return.As<Guid>($"n.{nameof(Operation.Id)}"),
					PipelineId = Return.As<Guid>($"n.{nameof(Operation.PipelineId)}"),
					Level = Return.As<int>("n._level"),
					OperationIdentifier = Return.As<string>($"n.{nameof(Operation.OperationIdentifier)}"),
					HashAtEnqueuing = Return.As<string>($"n.{nameof(Operation.OperationHash)}")
				})
				.OrderBy("n._level");

			var operationExecutionRecords = (await executionRecordsRequest.ResultsAsync)
				.Select(r => new OperationExecutionRecord
				{
					OperationId = r.OperationId,
					PipelineId = r.PipelineId,
					Level = r.Level,
					OperationIdentifier = r.OperationIdentifier,
					OperationHash = r.HashAtEnqueuing,
					Status = ExecutionStatus.ToBeExecuted
				}).ToList();

			executionRecord.OperationExecutionRecords = operationExecutionRecords;

			_context.OperationExecutionRecords.AddRange(operationExecutionRecords);
			_context.PipelineExecutionRecords.Add(executionRecord);
			await _context.SaveChangesAsync();

			_logger.LogInformation(
				"Created execution plan {ExecutionId} for pipeline {PipelineId} with {PartitionCount} partitions according to {ExecutionStrategy} strategy",
				executionRecord.Id, pipelineId, partitionResult.MaxLevel, strategy);

			return executionRecord;
		}

		public async Task<PipelineExecutionRecord> Get(Guid executionId, bool includeOperationRecords = true,
			bool reload = false)
		{
			var query = _context.PipelineExecutionRecords.Where(p => p.Id == executionId);

			if (includeOperationRecords)
			{
				_logger.LogDebug("Including operation records in query");
				query = query.Include(p => p.OperationExecutionRecords);
			}

			var record = await query.SingleOrDefaultAsync();

			if (record == null)
			{
				throw new NotFoundException($"Execution {executionId} not found");
			}

			// resolves an issue where data was not up-to-date when multiple threads use different instances of the db context
			// Specific case: synchronous execution of a pipeline that checks the execution status while "executed" messages are processed in another thread
			// TODO: either check if caching can resolve this issue, or get rid of synchronous execution completely
			if (reload)
			{
				_logger.LogDebug("Reloading execution records for execution {ExecutionId}", executionId);
				await _context.Entry(record).ReloadAsync();

				if (record.OperationExecutionRecords != null)
				{
					_logger.LogInformation("Reloading {OperationCount} operation records for execution {ExecutionId}",
						record.OperationExecutionRecords.Count, executionId);
					foreach (var operationExecutionRecord in record.OperationExecutionRecords)
					{
						await _context.Entry(operationExecutionRecord).ReloadAsync();
					}
				}
			}

			_logger.LogDebug("Loaded execution by id {ExecutionId}", executionId);

			return record;
		}

		public async Task<PipelineExecutionRecord> Update(PipelineExecutionRecord execution)
		{
			execution.ChangedOn = DateTime.UtcNow;

			_context.PipelineExecutionRecords.Update(execution);

			await _context.SaveChangesAsync();
			_logger.LogDebug("Updated pipeline execution record {ExecutionId}", execution.Id);

			return execution;
		}

		public async Task<PipelineExecutionRecord> GetLastExecutionForPipeline(Guid pipelineId)
		{
			var record = await _context.PipelineExecutionRecords
				.OrderByDescending(r => r.CompletedOn ?? r.CreatedOn)
				.Include(r => r.OperationExecutionRecords)
				.FirstOrDefaultAsync(exRec => exRec.PipelineId == pipelineId);

			if (record == default)
			{
				_logger.LogInformation("No execution record found for pipeline {PipelineId}", pipelineId);
				return null;
			}

			_logger.LogInformation("Loaded last execution record {ExecutionId} for pipeline {PipelineId}",
				record.Id, pipelineId);

			return record;
		}

		public async Task<OperationExecutionRecord> GetLastCompletedExecutionForOperation(Guid pipelineId, Guid operationId)
		{
			return await _context.OperationExecutionRecords
				.Where(op => op.PipelineExecutionRecord.PipelineId == pipelineId && op.OperationId == operationId)
				.OrderByDescending(op => op.ExecutionCompletedAt)
				.FirstOrDefaultAsync();
		}

		public async Task StoreExecutionHash(Guid executionId, Guid operationId,
			string operationHash, string predecessorsHash)
		{
			var operationExecutionRecord = await _context.OperationExecutionRecords
				.SingleOrDefaultAsync(op => op.OperationId == operationId && op.PipelineExecutionRecordId == executionId);
			if (operationExecutionRecord == default)
			{
				_logger.LogWarning("No operation execution record found for operation {OperationId} in execution {ExecutionId}",
					operationId, executionId);
				return;
			}

			operationExecutionRecord.OperationHash = operationHash;
			operationExecutionRecord.PredecessorsHash = predecessorsHash;

			await _context.SaveChangesAsync();
		}

		public async Task<int> DeleteExecutionRecords(Guid pipelineId)
		{
			_logger.LogDebug("Deleting pipeline execution records for pipeline {PipelineId}", pipelineId);

			var executionRecords = await _context.PipelineExecutionRecords
				.Where(r => r.PipelineId == pipelineId)
				.ToListAsync();

			_context.PipelineExecutionRecords.RemoveRange(executionRecords);
			await _context.SaveChangesAsync();

			_logger.LogInformation("Deleted {ExecutionCount} pipeline execution records for pipeline {PipelineId}",
				executionRecords.Count, pipelineId);

			return executionRecords.Count;
		}
	}
}
