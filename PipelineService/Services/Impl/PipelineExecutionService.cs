using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Exceptions;
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
        private readonly IMqttMessageService _mqttMessageService;

        public PipelineExecutionService(
            ILogger<PipelineExecutionService> logger,
            IPipelineDao pipelineDao,
            IPipelineExecutionDao pipelineExecutionDao,
            IMqttMessageService mqttMessageService)
        {
            _logger = logger;
            _pipelineDao = pipelineDao;
            _pipelineExecutionDao = pipelineExecutionDao;
            _mqttMessageService = mqttMessageService;
        }

        public async Task<IList<Pipeline>> CreateDefaultPipelines()
        {
            return await _pipelineDao.CreateDefaults();
        }

        public async Task<Pipeline> GetPipeline(Guid id)
        {
            return await _pipelineDao.Get(id);
        }

        public async Task<IList<Pipeline>> GetPipelines()
        {
            var pipelines = await _pipelineDao.Get();
            return pipelines
                .OrderByDescending(p => p.CreatedOn)
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

            await EnqueueNextBlocks(execution, pipeline);

            return execution.Id;
        }

        public async Task HandleExecutionResponse(BlockExecutionResponse response)
        {
            _logger.LogInformation(
                "Block ({BlockId}) completed for execution {ExecutionId} of pipeline {PipelineId} with success state {SuccessState} in {ExecutionTimeMs} ms",
                response.BlockId, response.ExecutionId, response.PipelineId, response.Successful,
                (int) (response.StopTime - response.StartTime).TotalMilliseconds);

            if (!response.Successful)
            {
                _logger.LogInformation("Execution of block {BlockId} failed with error {ExecutionErrorDescription}",
                    response.BlockId, response.ErrorDescription);

                // TODO mark execution as failed
                return;
            }

            if (await MarkBlockAsExecuted(response.ExecutionId, response.BlockId))
            {
                _logger.LogDebug("Nothing to enqueue at the moment");
            }
            else
            {
                var execution = await _pipelineExecutionDao.Get(response.ExecutionId);
                var pipeline = await _pipelineDao.Get(response.PipelineId);

                await EnqueueNextBlocks(execution, pipeline);
            }
        }

        private async Task EnqueueNextBlocks(PipelineExecutionRecord execution, Pipeline pipeline)
        {
            var toBeEnqueued = await SelectNextBlocks(execution.Id, pipeline);

            _logger.LogDebug("Enqueueing {ToBeEnqueued} blocks for execution {ExecutionId} of pipeline {PipelineId}",
                toBeEnqueued.Count, execution.Id, pipeline.Id);

            foreach (var block in toBeEnqueued)
            {
                await EnqueueBlock(execution.Id, block);
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
        private async Task<IList<Block>> SelectNextBlocks(Guid executionId, Pipeline pipeline)
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
                return new List<Block>();
            }

            if (executionRecord.ToBeExecuted.Count == 0)
            {
                _logger.LogInformation("No more blocks to execute");
                return new List<Block>();
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
                    "Moving block {BlockId} in execution {ExecutionId} from status to_be_executed to in_execution",
                    nextBlock.BlockId, executionId);
                executionRecord.ToBeExecuted.Remove(nextBlock);
                nextBlock.MovedToStatusInExecutionAt = DateTime.UtcNow;
                executionRecord.InExecution.Add(nextBlock);
            }

            return nextBlocks
                .Select(b => b.Block)
                .ToList();
        }

        /// <summary>
        /// Enqueues a block to be executed by the appropriate worker. 
        /// </summary>
        /// <param name="executionId">The execution this block belongs to.</param>
        /// <param name="block">The block to be executed.</param>
        private async Task EnqueueBlock(Guid executionId, Block block)
        {
            _logger.LogInformation("Enqueuing block ({BlockId}) with operation {Operation}", block.Id, block.Operation);

            BlockExecutionRequest request;
            // TODO: This can be solved in a nicer way by implementing eg the Visitor pattern 
            if (block.GetType() == typeof(SimpleBlock))
            {
                request = ExecutionRequestFromBlock(executionId, (SimpleBlock) block);
            }
            else
            {
                throw new InvalidOperationException($"Type {block.GetType()} is not supported");
            }

            await _mqttMessageService.PublishMessage($"execute/{block.PipelineId}", request);
        }

        /// <summary>
        /// Marks a block as executed in an execution.
        /// TODO: This method must become thread safe (case: multiple block finish execution at the same time -> whe updating data might get lost).
        /// </summary>
        /// <param name="executionId">The execution's id a block has been executed in.</param>
        /// <param name="blockId">The block that will be moved from status in execution to executed.</param>
        /// <returns>True if there are still blocks in status in_execution.</returns>
        private async Task<bool> MarkBlockAsExecuted(Guid executionId, Guid blockId)
        {
            PipelineExecutionRecord execution;
            try
            {
                execution = await _pipelineExecutionDao.Get(executionId);
            }
            catch (NotFoundException e)
            {
                throw new InvalidOperationException("Can not move block for non existent execution", e);
            }

            var block = execution.InExecution.FirstOrDefault(b => b.BlockId == blockId);

            if (block == null)
            {
                throw new InvalidOperationException("Block is not in status expected status for this execution");
            }

            _logger.LogDebug(
                "Moving block {BlockId} in execution {ExecutionId} from status in_execution to executed",
                blockId, executionId);

            block.ExecutionCompletedAt = DateTime.UtcNow;

            execution.InExecution.Remove(block);
            execution.Executed.Add(block);

            await _pipelineExecutionDao.Update(execution);

            return execution.InExecution.Count > 0;
        }

        private SimpleBlockExecutionRequest ExecutionRequestFromBlock(Guid executionId, SimpleBlock block)
        {
            // TODO this check should be moved to a more appropriate part of the code (eg. when creating an execution) 
            if (!block.InputDatasetId.HasValue && string.IsNullOrEmpty(block.InputDatasetHash))
            {
                _logger.LogWarning("Block {BlockId} has neither an input dataset id nor a producing block hash",
                    block.Id);
                throw new Exception("Block has neither an input dataset id nor a producing block hash");
            }

            return new SimpleBlockExecutionRequest
            {
                PipelineId = block.PipelineId,
                BlockId = block.Id,
                ExecutionId = executionId,
                OperationName = block.Operation,
                OperationConfiguration = block.OperationConfiguration,
                ResultKey = block.ResultKey,
                InputDataSetHash = block.InputDatasetHash,
                InputDataSetId = block.InputDatasetId,
            };
        }
    }
}