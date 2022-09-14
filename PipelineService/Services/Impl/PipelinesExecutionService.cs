using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Exceptions;
using PipelineService.Helper;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Services.Impl
{
	public class PipelinesExecutionService : IPipelineExecutionService
	{
		private string _operationExecuteTopic;

		private string OperationExecuteTopic => _operationExecuteTopic ??=
			_configuration.GetValue("QueueNames:OperationExecute", "operation/execute");

		private string DatasetDeletionTopic => _operationExecuteTopic ??=
			_configuration.GetValue("QueueNames:DatasetDelete", "dataset/delete");

		private readonly ILogger<PipelinesExecutionService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IPipelinesExecutionDao _pipelinesExecutionDao;
		private readonly EventBusService _eventBusService;
		private readonly EdgeEventBusService _edgeEventBusService;

		public PipelinesExecutionService(
			ILogger<PipelinesExecutionService> logger,
			IConfiguration configuration,
			IPipelinesDao pipelinesDao,
			IPipelinesExecutionDao pipelinesExecutionDao,
			EventBusService eventBusService,
			EdgeEventBusService edgeEventBusService)
		{
			_logger = logger;
			_configuration = configuration;
			_pipelinesDao = pipelinesDao;
			_pipelinesExecutionDao = pipelinesExecutionDao;
			_eventBusService = eventBusService;
			_edgeEventBusService = edgeEventBusService;
		}

		public async Task<IList<Pipeline>> CreateDefaultPipelines()
		{
			return await _pipelinesDao.CreatePipelines(HardcodedDefaultPipelines.NewDefaultPipelines());
		}

		public Task<IList<PipelineInfoDto>> GetTemplateInfoDtos()
		{
			_logger.LogDebug("Loading available pipeline templates");

			var templates = HardcodedDefaultPipelines.PipelineTemplates()
				.Select(p => new PipelineInfoDto
				{
					Id = p.Id,
					Name = p.Name
				})
				.ToList();

			return Task.FromResult<IList<PipelineInfoDto>>(templates);
		}

		public async Task<PipelineInfoDto> GetPipelineInfoDto(Guid id)
		{
			return await _pipelinesDao.GetInfoDto(id);
		}

		public async Task<PipelineInfoDto> DeletePipeline(Guid pipelineId)
		{
			_logger.LogDebug("Deleting pipeline {PipelineId}", pipelineId);
			var pipelineDto = await GetPipelineInfoDto(pipelineId);
			if (pipelineDto == null)
			{
				_logger.LogInformation("Could not delete pipeline {PipelineId}, because it does not exist", pipelineId);
				return null;
			}

			var datasets = (await _pipelinesDao.GetOperations(pipelineId)).SelectMany(o => o.Outputs).ToList();

			var deleted = await _pipelinesDao.DeletePipeline(pipelineId);
			if (deleted)
			{
				_logger.LogInformation("Deleted pipeline {PipelineId}", pipelineId);
				await DeleteDatasets(datasets);
			}
			else
			{
				_logger.LogInformation("Failed to delete pipeline {PipelineId}", pipelineId);
			}

			await _pipelinesExecutionDao.DeleteExecutionRecords(pipelineId);

			return pipelineDto;
		}

		private async Task DeleteDatasets(List<Dataset> datasets)
		{
			_logger.LogDebug("Marking {DatasetCount} datasets for deletion", datasets.Count);

			foreach (var dataset in datasets)
			{
				_logger.LogDebug("Marking dataset {DatasetKey} for deletion", dataset.Key);
				await _eventBusService.PublishMessage(DatasetDeletionTopic, new DatasetDeleteEvent
				{
					Dataset = dataset
				});
			}

			_logger.LogInformation("Marked {DatasetCount} datasets for deletion", datasets.Count);
		}

		public async Task<PipelineInfoDto> UpdatePipeline(PipelineInfoDto pipelineDto)
		{
			pipelineDto.ChangedOn = DateTime.UtcNow;
			return await _pipelinesDao.UpdatePipeline(pipelineDto);
		}

		public async Task<PipelineVisualizationDto> GetPipelineForVisualization(Guid pipelineId)
		{
			var dto = await _pipelinesDao.GetVisDto(pipelineId);
			if (dto == null)
			{
				_logger.LogDebug("Pipeline with id {PipelineId} not found", pipelineId);
				return null;
			}

			// foreach (var node in dto.Nodes.ToArray())
			// {
			// 	if (typeof(VisualizationOperationDto) == node.GetType())
			// 	{
			// 		foreach (var dataset in ((VisualizationOperationDto)node).Outputs)
			// 		{
			// 			var datasetNode = new VisualizationDatasetDto()
			// 			{
			// 				Id = Guid.NewGuid(),
			// 				Shape = "square",
			// 				Title = dataset.Type.ToString(),
			// 				Key = dataset.Key,
			// 				Store = dataset.Store,
			// 				Type = dataset.Type
			// 			};
			// 			dto.Nodes.Add(datasetNode);
			// 			dto.Edges.Add(new VisEdge()
			// 			{
			// 				From = node.Id,
			// 				To = datasetNode.Id
			// 			});
			// 			var targetNode = dto.Nodes
			// 				.Where(n => n.GetType() == typeof(VisualizationOperationDto))
			// 				.Select(n => (VisualizationOperationDto)n)
			// 				.FirstOrDefault(n => n.Inputs.Any(i => i.Key == dataset.Key));
			//
			// 			if (targetNode != default)
			// 			{
			// 				dto.Edges.Add(new VisEdge()
			// 				{
			// 					From = datasetNode.Id,
			// 					To = targetNode.Id
			// 				});
			// 			}
			// 		}
			// 	}
			// }

			return dto;
		}

		public async Task<PaginatedList<PipelineInfoDto>> GetPipelineDtos(Pagination pagination, string userIdentifier)
		{
			return await _pipelinesDao.GetDtos(pagination, userIdentifier);
		}

		public async Task<bool> HasBeenExecuted(Guid pipelineId)
		{
			_logger.LogDebug("Checking if pipeline {PipelineId} has been executed", pipelineId);
			var execution = await _pipelinesExecutionDao.GetLastExecutionForPipeline(pipelineId);
			return execution != null && execution.IsCompleted;
		}

		public async Task<Guid> ExecutePipeline(Guid pipelineId, bool skipIfExecuted = false,
			ExecutionStrategy strategy = ExecutionStrategy.Lazy)
		{
			_logger.LogInformation("Executing pipeline with id {PipelineId}", pipelineId);
			PipelineExecutionRecord execution;
			if (skipIfExecuted)
			{
				execution = await _pipelinesExecutionDao.GetLastExecutionForPipeline(pipelineId);
				if (execution != null && execution.IsCompleted)
				{
					_logger.LogInformation("Pipeline with id {PipelineId} has already been executed, skipping", pipelineId);
					return execution.Id;
				}
			}

			var pipeline = await _pipelinesDao.GetInfoDto(pipelineId);
			pipeline.LastRunStart = DateTime.UtcNow;
			pipeline.LastRunSuccess = null;
			pipeline.LastRunFailure = null;
			await _pipelinesDao.UpdatePipeline(pipeline);

			execution = await _pipelinesExecutionDao.Create(pipelineId, strategy);

			await EnqueueNextOperations(execution, pipelineId);

			return execution.Id;
		}

		public async Task<PipelineExecutionRecord> ExecutePipelineSync(Guid pipelineId, bool skipIfExecuted = false)
		{
			if (skipIfExecuted)
			{
				var executionRecord = await _pipelinesExecutionDao.GetLastExecutionForPipeline(pipelineId);
				if (executionRecord != null && executionRecord.IsCompleted)
				{
					_logger.LogInformation("Pipeline with id {PipelineId} has already been executed, skipping", pipelineId);
					return executionRecord;
				}
			}

			var executionId = await ExecutePipeline(pipelineId);

			var i = 0;
			var startTimestamp = DateTime.UtcNow;
			while (true)
			{
				var executionRecord = await _pipelinesExecutionDao.Get(executionId, true, true);
				if (executionRecord.IsCompleted)
				{
					_logger.LogInformation("Pipeline with id {PipelineId} has been executed (sync)", pipelineId);
					return executionRecord;
				}

				i++;
				if ((DateTime.UtcNow - startTimestamp).TotalMinutes >
				    _configuration.GetValue("PipelineExecutionService:MaxExecutionTimeSync", 2))
				{
					_logger.LogWarning("Aborting waiting for pipeline execution to complete, because it took too long");
					return executionRecord;
				}

				var pollingTimeout = _configuration.GetValue("PipelineExecutionService:SyncPollingTimeout", 4);

				if (i % 10 == 0)
				{
					_logger.LogInformation(
						"Waiting for sync pipeline execution (pipelineId: {PipelineId} executionId: {ExecutionId}) to complete (sync) since {PollingTimeout}s",
						pipelineId, executionId, (DateTime.UtcNow - startTimestamp).TotalSeconds);
				}
				else
				{
					_logger.LogDebug(
						"Waiting for sync pipeline execution (pipelineId: {PipelineId} executionId: {ExecutionId}) to complete (sync) since {PollingTimeout}s",
						pipelineId, executionId, (DateTime.UtcNow - startTimestamp).TotalSeconds);
				}

				await Task.Delay(pollingTimeout * 1000);
			}
		}

		public async Task HandleExecutionResponse(OperationExecutedMessage response)
		{
			_logger.LogInformation(
				"Operation ({OperationId}) completed for execution {ExecutionId} of pipeline {PipelineId} with success state {SuccessState} in {ExecutionTimeMs} ms",
				response.OperationId, response.ExecutionId, response.PipelineId, response.Successful,
				(int)(response.StopTime - response.StartTime).TotalMilliseconds);

			PipelineExecutionRecord execution = null;

			if (!response.Successful)
			{
				_logger.LogInformation("Execution of operation {OperationId} failed with error {ExecutionErrorDescription}",
					response.OperationId, response.ErrorDescription);

				await MarkOperationAsFailed(response.ExecutionId, response.OperationId);
			}
			else
			{
				var waitingForOperationsToComplete = await MarkOperationAsExecuted(response);
				if (waitingForOperationsToComplete)
				{
					_logger.LogDebug("Nothing to enqueue at the moment...");
				}
				else
				{
					execution = await _pipelinesExecutionDao.Get(response.ExecutionId);
					await EnqueueNextOperations(execution, response.PipelineId);
				}
			}

			if (execution == null)
			{
				_logger.LogDebug("Loading execution record from database since it was not needed before...");
				execution = await _pipelinesExecutionDao.Get(response.ExecutionId);
			}

			if (execution.IsCompleted)
			{
				var pipeline = await _pipelinesDao.GetInfoDto(response.PipelineId);
				if (execution.IsSuccessful)
				{
					pipeline.LastRunSuccess = DateTime.UtcNow;
					_logger.LogInformation("Pipeline {PipelineId} has been successfully executed", response.PipelineId);
				}
				else
				{
					pipeline.LastRunFailure = DateTime.UtcNow;
					_logger.LogInformation("Pipeline {PipelineId} has failed", response.PipelineId);
				}

				await _pipelinesDao.UpdatePipeline(pipeline);
			}

			await NotifyFrontend(response);
		}

		public async Task<CreateFromTemplateResponse> CreatePipelineFromTemplate(CreateFromTemplateRequest request)
		{
			_logger.LogDebug("Creating a new pipeline for user {UserIdentifier} from template {PipelineTemplateId}",
				request.UserIdentifier, request.TemplateId);

			var response = new CreateFromTemplateResponse();

			var template = !request.TemplateId.HasValue
				? HardcodedDefaultPipelines.EmptyTemplate()
				: HardcodedDefaultPipelines.PipelineTemplates().SingleOrDefault(t => t.Id == request.TemplateId);

			if (template == null)
			{
				response.Success = false;
				return response;
			}

			template.Id = Guid.NewGuid();
			SetPipelineIdForAllOperations(template.Id, template.Root);
			template.UserIdentifier = request.UserIdentifier;
			template.CreatedOn = DateTime.UtcNow;

			await _pipelinesDao.CreatePipelines(new List<Pipeline> { template });

			response.PipelineId = template.Id;
			response.Success = true;

			return response;
		}

		private static void SetPipelineIdForAllOperations(Guid pipelineId, IList<Operation> templateRoot)
		{
			foreach (var operation in templateRoot)
			{
				operation.PipelineId = pipelineId;
				SetPipelineIdForAllOperations(pipelineId, operation.Successors);
			}
		}

		private async Task NotifyFrontend(OperationExecutedMessage response)
		{
			var executionRecord = await _pipelinesExecutionDao.Get(response.ExecutionId);
			var operationExecutionRecord =
				executionRecord.OperationExecutionRecords.FirstOrDefault(b =>
					b.OperationId == response.OperationId && b.Status == ExecutionStatus.Succeeded) ??
				executionRecord.OperationExecutionRecords.FirstOrDefault(b =>
					b.OperationId == response.OperationId && b.Status == ExecutionStatus.Failed);

			await _edgeEventBusService.PublishMessage($"pipeline/event/{response.PipelineId}",
				new FrontendExecutionNotification
				{
					PipelineId = response.PipelineId,
					ExecutionId = response.ExecutionId,
					OperationId = response.OperationId,
					OperationName = operationExecutionRecord?.OperationIdentifier,
					Successful = response.Successful,
					Cached = response.Cached,
					CompletedAt = response.StopTime,
					ExecutionTime = (response.StopTime - response.StartTime).Milliseconds,
					ErrorDescription = response.ErrorDescription,
					OperationsExecuted =
						executionRecord.OperationExecutionRecords.Count(o => o.Status == ExecutionStatus.Succeeded),
					OperationsInExecution =
						executionRecord.OperationExecutionRecords.Count(o => o.Status == ExecutionStatus.InExecution),
					OperationsToBeExecuted =
						executionRecord.OperationExecutionRecords.Count(o => o.Status == ExecutionStatus.ToBeExecuted),
					OperationsFailedToExecute =
						executionRecord.OperationExecutionRecords.Count(o => o.Status == ExecutionStatus.Failed),
					ResultDatasets = operationExecutionRecord != null
						? await _pipelinesDao.GetOutputDatasets(operationExecutionRecord.OperationId)
						: new List<Dataset>(),
				});
		}

		private async Task EnqueueNextOperations(PipelineExecutionRecord execution, Guid pipelineId)
		{
			var operationsToBeEnqueued = await SelectNextOperations(execution, pipelineId);

			_logger.LogDebug("Enqueueing {ToBeEnqueued} operations for execution {ExecutionId} of pipeline {PipelineId}",
				operationsToBeEnqueued.Count, execution.Id, pipelineId);

			foreach (var operationToBeEnqueued in operationsToBeEnqueued)
			{
				await EnqueueOperation(execution.Id, operationToBeEnqueued.OperationId);
			}
		}

		/// <summary>
		/// Selects the next operations to be executed for a given execution of a pipeline.
		/// Might return empty list if no operations need to be executed at the moment.
		/// </summary>
		///
		/// <exception cref="InvalidOperationException">If no execution for a given execution id exists.</exception>
		/// <exception cref="ArgumentException">If the execution does not match the pipeline.</exception>
		/// <param name="executionRecord">The pipeline's execution record.</param>
		/// <param name="pipelineId">The pipeline that is being executed.</param>
		/// <returns>A list of operations that need to be executed next inorder to complete the execution of the pipeline</returns>
		private async Task<List<OperationExecutionRecord>> SelectNextOperations(PipelineExecutionRecord executionRecord,
			Guid pipelineId)
		{
			if (executionRecord.PipelineId != pipelineId)
			{
				throw new ArgumentException("PipelineId in loaded execution does not match pipelineId",
					nameof(executionRecord.Id));
			}

			if (executionRecord.WaitingForOperations)
			{
				_logger.LogInformation("There are operations currently in execution -> no operations can be selected");
				return new List<OperationExecutionRecord>();
			}

			if (executionRecord.IsCompleted)
			{
				_logger.LogInformation("All operations have already been executed");
				return new List<OperationExecutionRecord>();
			}

			var currentLevel = executionRecord.OperationExecutionRecords
				.Where(o => o.Status == ExecutionStatus.ToBeExecuted)
				.Min(o => o.Level);

			var nextOperations = executionRecord.OperationExecutionRecords
				.Where(b => b.Level == currentLevel)
				.ToList();

			_logger.LogDebug("Executing {ExecutionLevelCount} operations from level {ExecutionLevel}",
				nextOperations.Count, currentLevel);

			_logger.LogInformation(
				"Changing operation execution record status to {OperationExecutionStatus} for operations with level {OperationExecutionLevel} execution {ExecutionId} of pipeline {PipelineId}",
				ExecutionStatus.InExecution, currentLevel, executionRecord.Id, pipelineId);
			foreach (var nextOperation in nextOperations)
			{
				_logger.LogDebug(
					"Moving operation {OperationIdentifier} ({OperationId}) in execution {ExecutionId} from status {OperationExecutionStatusFrom} to {OperationExecutionStatusTo}",
					nextOperation.OperationIdentifier, nextOperation.OperationId, executionRecord.Id, nextOperation.Status,
					ExecutionStatus.InExecution);
				nextOperation.Status = ExecutionStatus.InExecution;
				nextOperation.MovedToStatusInExecutionAt = DateTime.UtcNow;
			}

			await _pipelinesExecutionDao.Update(executionRecord);

			return nextOperations;
		}

		/// <summary>
		/// Enqueues an operation to be executed by the appropriate worker.
		/// </summary>
		/// <param name="executionId">The execution's id this operation belongs to.</param>
		/// <param name="operationId">The operation's id to be executed.</param>
		private async Task EnqueueOperation(Guid executionId, Guid operationId)
		{
			var startTime = DateTime.UtcNow;
			var operation = await _pipelinesDao.GetOperation(operationId);
			var predecessorHashes = await _pipelinesDao.GetPredecessorHashes(operationId);
			var predHashCombined = HashHelper.ComputeHash(predecessorHashes);
			if (predHashCombined != operation.PredecessorsHash)
			{
				operation.PredecessorsHash = predHashCombined;
				await _pipelinesDao.UpdateOperation(operation);
				_logger.LogInformation("Updating predecessor hashes for operation {OperationId} {PredecessorsHash}",
					operation.Id, predHashCombined);
			}

			await _pipelinesExecutionDao.StoreExecutionHash(executionId, operationId,
				operation.OperationHash, operation.PredecessorsHash);
			var lastExecutionRecord =
				await _pipelinesExecutionDao.GetLastCompletedExecutionForOperation(operation.PipelineId, operationId);

			if (lastExecutionRecord != null &&
			    lastExecutionRecord.IsSuccessful &&
			    lastExecutionRecord.OperationHash == operation.OperationHash &&
			    lastExecutionRecord.PredecessorsHash == operation.PredecessorsHash)
			{
				_logger.LogInformation(
					"Skipping already executed operation {OperationId} with operation identifier {Operation}",
					operation.Id, operation.OperationIdentifier);

				await HandleExecutionResponse(new OperationExecutedMessage
				{
					PipelineId = operation.PipelineId,
					ExecutionId = executionId,
					OperationId = operationId,
					Successful = true,
					StartTime = startTime,
					StopTime = DateTime.UtcNow,
					Cached = true
				});
			}
			else
			{
				_logger.LogInformation("Enqueuing operation {OperationId} with operation identifier {Operation}",
					operation.Id, operation.OperationIdentifier);

				var message = ExecutionRequestFromOperation(executionId, operation);

				await _eventBusService.PublishMessage(OperationExecuteTopic, message);
			}
		}

		/// <summary>
		/// Marks a node as executed in an execution.
		/// TODO: This method must become thread safe (case: multiple node finish execution at the same time -> when updating data might get lost).
		/// </summary>
		/// <returns>True if there are still blocks in status in_execution.</returns>
		private async Task<bool> MarkOperationAsExecuted(OperationExecutedMessage executedMessage)
		{
			PipelineExecutionRecord execution;
			try
			{
				execution = await _pipelinesExecutionDao.Get(executedMessage.ExecutionId);
			}
			catch (NotFoundException e)
			{
				throw new InvalidOperationException("Can not move node for non existent execution", e);
			}

			var operationExecRec = execution.OperationExecutionRecords
				.FirstOrDefault(b => b.OperationId == executedMessage.OperationId && b.Status == ExecutionStatus.InExecution);

			if (operationExecRec == null)
			{
				throw new InvalidOperationException("Operation is not in status expected status for this execution");
			}

			_logger.LogDebug(
				"Moving operation {OperationId} in execution {ExecutionId} from status {OperationExecutionStatusFrom} to {OperationExecutionStatusTo}",
				executedMessage.OperationId, executedMessage.ExecutionId, operationExecRec.Status, ExecutionStatus.Succeeded);

			operationExecRec.ExecutionCompletedAt = DateTime.UtcNow;
			operationExecRec.Status = ExecutionStatus.Succeeded;
			operationExecRec.Cached = executedMessage.Cached;

			CheckIfCompleted(execution);

			await _pipelinesExecutionDao.Update(execution);

			return execution.OperationExecutionRecords.Any(o => o.Status == ExecutionStatus.InExecution);
		}

		private async Task MarkOperationAsFailed(Guid responseExecutionId, Guid responseOperationId)
		{
			PipelineExecutionRecord execution;
			try
			{
				execution = await _pipelinesExecutionDao.Get(responseExecutionId);
			}
			catch (NotFoundException e)
			{
				throw new InvalidOperationException("Can not move node for non existent execution", e);
			}

			var operation = execution.OperationExecutionRecords
				.FirstOrDefault(b => b.OperationId == responseOperationId && b.Status == ExecutionStatus.InExecution);

			if (operation == null)
			{
				throw new InvalidOperationException("Operation is not in status expected status for this execution");
			}

			_logger.LogDebug(
				"Moving operation {OperationId} in execution {ExecutionId} from status {OperationExecutionStatusFrom} to {OperationExecutionStatusTo}",
				responseOperationId, responseExecutionId, operation.Status, ExecutionStatus.Failed);

			operation.ExecutionCompletedAt = DateTime.UtcNow;
			operation.Status = ExecutionStatus.Failed;

			_logger.LogDebug("Moving all enqueued operations to failed state");
			// Optimization: Only move enqueued execution operations records to failed state if they depend on the failed execution (see implementations before 2021/11/09).
			foreach (var executionRecord in execution.OperationExecutionRecords.Where(o =>
				         o.Status == ExecutionStatus.ToBeExecuted))
			{
				_logger.LogDebug(
					"Moving operation {OperationId} in execution {ExecutionId} from status {OperationExecutionStatusFrom} to {OperationExecutionStatusTo}",
					executionRecord.OperationId, responseExecutionId, operation.Status, ExecutionStatus.Failed);

				executionRecord.ExecutionCompletedAt = DateTime.UtcNow;
				executionRecord.Status = ExecutionStatus.Failed;
			}

			CheckIfCompleted(execution);

			await _pipelinesExecutionDao.Update(execution);
		}

		private static void CheckIfCompleted(PipelineExecutionRecord execution)
		{
			if (execution.IsCompleted)
			{
				execution.CompletedOn = DateTime.UtcNow;
				execution.CompletionStatus = execution.IsSuccessful ? ExecutionStatus.Succeeded : ExecutionStatus.Failed;
			}
		}

		private OperationExecutionMessage ExecutionRequestFromOperation(Guid executionId, Operation operation)
		{
			return new OperationExecutionMessage
			{
				PipelineId = operation.PipelineId,
				ExecutionId = executionId,
				OperationId = operation.Id,
				WorkerOperationId = operation.OperationId,
				WorkerOperationIdentifier = operation.OperationIdentifier,
				OperationConfiguration = operation.OperationConfiguration,
				Inputs = operation.Inputs,
				Outputs = operation.Outputs
			};
		}
	}
}
