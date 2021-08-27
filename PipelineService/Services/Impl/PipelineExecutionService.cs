using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Services.Impl
{
    public class PipelineExecutionService : IPipelineExecutionService
    {
        private readonly ILogger<PipelineExecutionService> _logger;
        private readonly IPipelineDao _pipelineDao;
        private readonly IPipelineExecutionDao _pipelineExecutionDao;
        private readonly EventBusService _eventBusService;
        private readonly EdgeEventBusService _edgeEventBusService;

        public PipelineExecutionService(
            ILogger<PipelineExecutionService> logger,
            IPipelineDao pipelineDao,
            IPipelineExecutionDao pipelineExecutionDao,
            EventBusService eventBusService,
            EdgeEventBusService edgeEventBusService)
        {
            _logger = logger;
            _pipelineDao = pipelineDao;
            _pipelineExecutionDao = pipelineExecutionDao;
            _eventBusService = eventBusService;
            _edgeEventBusService = edgeEventBusService;
        }

        public async Task<IList<Pipeline>> CreateDefaultPipelines()
        {
            return await _pipelineDao.CreateDefaults();
        }

        public async Task<Pipeline> GetPipeline(Guid id)
        {
            return await _pipelineDao.Get(id);
        }

        public async Task<IList<PipelineSummaryDto>> GetPipelines()
        {
            var pipelines = await _pipelineDao.Get();
            return pipelines
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new PipelineSummaryDto
                {
                    Name = p.Name,
                    Id = p.Id,
                    CreatedOn = p.CreatedOn
                })
                .ToList();
        }

        public async Task<Guid> ExecutePipeline(Guid pipelineId)
        {
            _logger.LogInformation("Executing pipeline with id {PipelineId}", pipelineId);
            var pipeline = await _pipelineDao.Get(pipelineId);

            if (pipeline == null)
            {
                throw new NotFoundException("No pipeline with provided id found");
            }

            var execution = await _pipelineExecutionDao.Create(pipeline);

            await EnqueueNextNodes(execution, pipeline);

            return execution.Id;
        }

        public async Task HandleExecutionResponse(NodeExecutionResponse response)
        {
            _logger.LogInformation(
                "Node ({NodeId}) completed for execution {ExecutionId} of pipeline {PipelineId} with success state {SuccessState} in {ExecutionTimeMs} ms",
                response.NodeId, response.ExecutionId, response.PipelineId, response.Successful,
                (int)(response.StopTime - response.StartTime).TotalMilliseconds);

            if (!response.Successful)
            {
                _logger.LogInformation("Execution of node {NodeId} failed with error {ExecutionErrorDescription}",
                    response.NodeId, response.ErrorDescription);

                await MarkNodeAsFailed(response.ExecutionId, response.NodeId);
                await NotifyFrontend(response);
                return;
            }

            if (await MarkNodeAsExecuted(response.ExecutionId, response.NodeId))
            {
                _logger.LogDebug("Nothing to enqueue at the moment");
            }
            else
            {
                var execution = await _pipelineExecutionDao.Get(response.ExecutionId);
                var pipeline = await _pipelineDao.Get(response.PipelineId);

                await EnqueueNextNodes(execution, pipeline);
            }

            await NotifyFrontend(response);
        }

        // TODO this method should be independent of response type -> no switch for types
        private async Task NotifyFrontend(NodeExecutionResponse response)
        {
            var executionRecord = await _pipelineExecutionDao.Get(response.ExecutionId);
            var blockExecutionRecord = executionRecord.Executed.FirstOrDefault(b => b.NodeId == response.NodeId);
            if (blockExecutionRecord == null)
            {
                blockExecutionRecord = executionRecord.Failed.FirstOrDefault(b => b.NodeId == response.NodeId);
            }

            string resultKey = null;
            switch (blockExecutionRecord?.Node)
            {
                case NodeFileInput fileInput:
                    resultKey = fileInput.ResultKey;
                    break;
                case NodeSingleInput singleInputNode:
                    resultKey = singleInputNode.ResultKey;
                    break;
                case NodeDoubleInput doubleInputNode:
                    resultKey = doubleInputNode.ResultKey;
                    break;
            }

            await _edgeEventBusService.PublishMessage($"pipeline/event/{response.PipelineId}",
                new FrontendExecutionNotification
                {
                    PipelineId = response.PipelineId,
                    ExecutionId = response.ExecutionId,
                    NodeId = response.NodeId,
                    OperationName = blockExecutionRecord?.Name,
                    Successful = response.Successful,
                    CompletedAt = response.StopTime,
                    ExecutionTime = (response.StopTime - response.StartTime).Milliseconds,
                    ErrorDescription = response.ErrorDescription,
                    NodesExecuted = executionRecord.Executed.Count,
                    NodesInExecution = executionRecord.InExecution.Count,
                    NodesToBeExecuted = executionRecord.ToBeExecuted.Count,
                    NodesFailedToExecute = executionRecord.Failed.Count,
                    ResultDatasetKey = resultKey
                });
        }

        private async Task EnqueueNextNodes(PipelineExecutionRecord execution, Pipeline pipeline)
        {
            var toBeEnqueued = await SelectNextNodes(execution.Id, pipeline);

            _logger.LogDebug("Enqueueing {ToBeEnqueued} blocks for execution {ExecutionId} of pipeline {PipelineId}",
                toBeEnqueued.Count, execution.Id, pipeline.Id);

            foreach (var block in toBeEnqueued)
            {
                await EnqueueNode(execution.Id, block);
            }
        }

        /// <summary>
        /// Selects the next blocks to be executed for a given execution of a pipeline.
        /// Might return empty list if no blocks need to be executed at the moment.
        /// </summary>
        /// 
        /// <exception cref="InvalidOperationException">If no execution for a given execution id exists.</exception>
        /// <exception cref="ArgumentException">If the execution does not match the pipeline.</exception>
        /// <param name="executionId">The execution's id.</param>
        /// <param name="pipeline">The pipeline that is being executed.</param>
        /// <returns>A list of blocks that need to be executed next inorder to complete the execution of the pipeline</returns>
        private async Task<IList<Node>> SelectNextNodes(Guid executionId, Pipeline pipeline)
        {
            PipelineExecutionRecord executionRecord;
            try
            {
                executionRecord = await _pipelineExecutionDao.Get(executionId);
            }
            catch (NotFoundException e)
            {
                throw new InvalidOperationException("Can not select blocks for non existent execution", e);
            }

            if (executionRecord.PipelineId != pipeline.Id)
            {
                throw new ArgumentException("Pipeline id in loaded execution does not match pipeline id",
                    nameof(executionId));
            }

            if (executionRecord.InExecution.Count != 0)
            {
                _logger.LogInformation("Blocks in execution -> no blocks can be selected");
                return new List<Node>();
            }

            if (executionRecord.ToBeExecuted.Count == 0)
            {
                _logger.LogInformation("No more blocks to execute");
                return new List<Node>();
            }

            var currentLevel = executionRecord.ToBeExecuted[0].Level;

            var nextBlocks = executionRecord.ToBeExecuted
                .Where(b => b.Level == currentLevel)
                .ToList();

            _logger.LogDebug("Executing {ExecutionLevelCount} blocks from level {ExecutionLevel}",
                nextBlocks.Count, currentLevel);

            // moving blocks from to be executed list to in execution list
            foreach (var nextBlock in nextBlocks)
            {
                _logger.LogDebug(
                    "Moving node {NodeId} in execution {ExecutionId} from status to_be_executed to in_execution",
                    nextBlock.NodeId, executionId);
                executionRecord.ToBeExecuted.Remove(nextBlock);
                nextBlock.MovedToStatusInExecutionAt = DateTime.UtcNow;
                executionRecord.InExecution.Add(nextBlock);
            }

            return nextBlocks
                .Select(b => b.Node)
                .ToList();
        }

        /// <summary>
        /// Enqueues a node to be executed by the appropriate worker. 
        /// </summary>
        /// <param name="executionId">The execution this node belongs to.</param>
        /// <param name="node">The node to be executed.</param>
        private async Task EnqueueNode(Guid executionId, Node node)
        {
            _logger.LogInformation("Enqueuing node ({NodeId}) with operation {Operation}", node.Id, node.Operation);

            NodeExecutionRequest request;
            // TODO: This can be solved in a nicer way by implementing eg the Visitor pattern 
            if (node.GetType() == typeof(NodeFileInput))
            {
                request = ExecutionRequestFromNode(executionId, (NodeFileInput)node);
                await _eventBusService.PublishMessage($"execute/{node.PipelineId}/file", request);
            }
            else if (node.GetType() == typeof(NodeSingleInput))
            {
                request = ExecutionRequestFromNode(executionId, (NodeSingleInput)node);
                await _eventBusService.PublishMessage($"execute/{node.PipelineId}/single", request);
            }
            else if (node.GetType() == typeof(NodeDoubleInput))
            {
                request = ExecutionRequestFromNode(executionId, (NodeDoubleInput)node);
                await _eventBusService.PublishMessage($"execute/{node.PipelineId}/double", request);
            }
            else
            {
                throw new InvalidOperationException($"Type {node.GetType()} is not supported");
            }
        }

        /// <summary>
        /// Marks a node as executed in an execution.
        /// TODO: This method must become thread safe (case: multiple node finish execution at the same time -> when updating data might get lost).
        /// </summary>
        /// <param name="executionId">The execution's id a node has been executed in.</param>
        /// <param name="blockId">The node that will be moved from status in execution to executed.</param>
        /// <returns>True if there are still blocks in status in_execution.</returns>
        private async Task<bool> MarkNodeAsExecuted(Guid executionId, Guid blockId)
        {
            PipelineExecutionRecord execution;
            try
            {
                execution = await _pipelineExecutionDao.Get(executionId);
            }
            catch (NotFoundException e)
            {
                throw new InvalidOperationException("Can not move node for non existent execution", e);
            }

            var block = execution.InExecution.FirstOrDefault(b => b.NodeId == blockId);

            if (block == null)
            {
                throw new InvalidOperationException("Node is not in status expected status for this execution");
            }

            _logger.LogDebug(
                "Moving node {NodeId} in execution {ExecutionId} from status in_execution to executed",
                blockId, executionId);

            block.ExecutionCompletedAt = DateTime.UtcNow;

            execution.InExecution.Remove(block);
            execution.Executed.Add(block);

            await _pipelineExecutionDao.Update(execution);

            return execution.InExecution.Count > 0;
        }

        private async Task MarkNodeAsFailed(Guid responseExecutionId, Guid responseBlockId)
        {
            PipelineExecutionRecord execution;
            try
            {
                execution = await _pipelineExecutionDao.Get(responseExecutionId);
            }
            catch (NotFoundException e)
            {
                throw new InvalidOperationException("Can not move node for non existent execution", e);
            }

            var block = execution.InExecution.FirstOrDefault(b => b.NodeId == responseBlockId);

            if (block == null)
            {
                throw new InvalidOperationException("Node is not in status expected status for this execution");
            }

            _logger.LogDebug(
                "Moving node {NodeId} in execution {ExecutionId} from status in_execution to failed",
                responseBlockId, responseExecutionId);

            block.ExecutionCompletedAt = DateTime.UtcNow;

            execution.InExecution.Remove(block);
            execution.Failed.Add(block);

            _logger.LogDebug("Moving all operations following failed to operation to failed state");

            var successorIds = GetAllSuccessorIds(block.Node.Successors);
            var failedBlocks = execution.ToBeExecuted
                .Where(e => successorIds.Contains(e.NodeId))
                .ToList();

            foreach (var failedBlock in failedBlocks)
            {
                execution.ToBeExecuted.Remove(failedBlock);
                execution.Failed.Add(failedBlock);
            }

            await _pipelineExecutionDao.Update(execution);
        }

        private static IList<Guid> GetAllSuccessorIds(IList<Node> blockSuccessors, List<Guid> ids = default)
        {
            ids ??= new List<Guid>();

            ids.AddRange(blockSuccessors.Select(b => b.Id));

            foreach (var blockSuccessor in blockSuccessors)
            {
                GetAllSuccessorIds(blockSuccessor.Successors, ids);
            }

            return ids;
        }

        private NodeExecutionRequest ExecutionRequestFromNode(Guid executionId, NodeFileInput node)
        {
            // TODO this check should be moved to a more appropriate part of the code (eg. when creating an execution) 
            if (string.IsNullOrEmpty(node.InputObjectKey) && string.IsNullOrEmpty(node.InputObjectBucket))
            {
                _logger.LogWarning("Node {NodeId} has no input object key or bucket defined",
                    node.Id);
                throw new ValidationException("Node has neither an input dataset id nor a producing node hash");
            }

            return new NodeExecutionRequestFileInput
            {
                PipelineId = node.PipelineId,
                NodeId = node.Id,
                ExecutionId = executionId,
                OperationName = node.Operation,
                OperationId = node.OperationId,
                OperationConfiguration = node.OperationConfiguration,
                ResultKey = node.ResultKey,
                InputObjectBucket = node.InputObjectBucket,
                InputObjectKey = node.InputObjectKey
            };
        }

        private NodeExecutionRequest ExecutionRequestFromNode(Guid executionId, NodeSingleInput node)
        {
            // TODO this check should be moved to a more appropriate part of the code (eg. when creating an execution) 
            if (!node.InputDatasetId.HasValue && string.IsNullOrEmpty(node.InputDatasetHash))
            {
                _logger.LogWarning("Node {NodeId} has neither an input dataset id nor a producing node hash",
                    node.Id);
                throw new ValidationException("Node has neither an input dataset id nor a producing node hash");
            }

            return new NodeExecutionRequestSingleInput
            {
                PipelineId = node.PipelineId,
                NodeId = node.Id,
                ExecutionId = executionId,
                OperationName = node.Operation,
                OperationId = node.OperationId,
                OperationConfiguration = node.OperationConfiguration,
                ResultKey = node.ResultKey,
                InputDataSetHash = node.InputDatasetHash,
                InputDataSetId = node.InputDatasetId,
            };
        }

        private NodeExecutionRequest ExecutionRequestFromNode(Guid executionId, NodeDoubleInput node)
        {
            // TODO this check should be moved to a more appropriate part of the code (eg. when creating an execution) 
            if (!node.InputDatasetOneId.HasValue && string.IsNullOrEmpty(node.InputDatasetOneHash))
            {
                _logger.LogWarning(
                    "Node {NodeId} has neither an input dataset id nor a producing node hash for the first dataset",
                    node.Id);
                throw new ValidationException("Node has neither an input dataset id nor a producing node hash");
            }

            if (!node.InputDatasetTwoId.HasValue && string.IsNullOrEmpty(node.InputDatasetTwoHash))
            {
                _logger.LogWarning(
                    "Node {NodeId} has neither an input dataset id nor a producing node hash for the second dataset",
                    node.Id);
                throw new ValidationException("Node has neither an input dataset id nor a producing node hash");
            }

            return new NodeExecutionRequestDoubleInput
            {
                PipelineId = node.PipelineId,
                NodeId = node.Id,
                ExecutionId = executionId,
                OperationName = node.Operation,
                OperationId = node.OperationId,
                OperationConfiguration = node.OperationConfiguration,
                ResultKey = node.ResultKey,
                InputDataSetOneHash = node.InputDatasetOneHash,
                InputDataSetOneId = node.InputDatasetOneId,
                InputDataSetTwoHash = node.InputDatasetTwoHash,
                InputDataSetTwoId = node.InputDatasetTwoId
            };
        }
    }
}