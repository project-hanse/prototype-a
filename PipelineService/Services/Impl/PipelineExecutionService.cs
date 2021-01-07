using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Helper;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Services.Impl
{
    public class PipelineExecutionService : IPipelineExecutionService
    {
        // TODO change this to persistent storage.
        private static readonly IDictionary<Guid, PipelineExecutionRecord> Store =
            new ConcurrentDictionary<Guid, PipelineExecutionRecord>();

        private readonly ILogger<PipelineExecutionService> _logger;
        private readonly IPipelineService _pipelineService;
        private readonly IMqttMessageService _mqttMessageService;
        private readonly IHashService _hashService;

        public PipelineExecutionService(
            ILogger<PipelineExecutionService> logger,
            IPipelineService pipelineService,
            IMqttMessageService mqttMessageService,
            IHashService hashService)
        {
            _logger = logger;
            _pipelineService = pipelineService;
            _mqttMessageService = mqttMessageService;
            _hashService = hashService;
        }

        public Task<PipelineExecutionRecord> GetById(Guid executionId)
        {
            if (!Store.TryGetValue(executionId, out var pipelineExecutionRecord))
            {
                throw new NotFoundException("No PipelineExecutionRecord with id found");
            }

            _logger.LogInformation("Loaded execution by id {executionId}", executionId);
            return Task.FromResult(pipelineExecutionRecord);
        }

        public async Task<Guid?> ExecutePipeline(Guid pipelineId)
        {
            _logger.LogInformation("Executing pipeline with id {pipelineId}", pipelineId);
            var pipeline = await _pipelineService.GetPipeline(pipelineId);

            if (pipeline == null)
            {
                _logger.LogWarning("No pipeline with id {pipelineId} found", pipelineId);
                return null;
            }

            var execution = new PipelineExecutionRecord
            {
                PipelineId = pipeline.Id
            };


            return execution.Id;
        }

        public async Task EnqueueBlock(Guid executionId, Block block)
        {
            _logger.LogInformation("Enqueuing block ({blockId}) with operation {operation}", block.Id, block.Operation);

            var message = new SimpleBlockExecutionRequest
            {
                PipelineId = block.PipelineId,
                BlockId = block.Id,
                ExecutionId = executionId,
                OperationName = block.Operation,
                OperationConfiguration = block.OperationConfiguration,
                ProducingHash = _hashService.ComputeHash(block)
            };

            // TODO this needs to change
            if (message.GetType() == typeof(SimpleBlock))
            {
                var inputDatasetId = ((SimpleBlock) block).InputDatasetId;
                if (inputDatasetId != null)
                    message.InputDataSetIds = new List<Guid> {inputDatasetId.Value};
            }

            await _mqttMessageService.PublishMessage($"execute/{block.PipelineId}", message);

            await _mqttMessageService.Subscribe($"executed/{block.PipelineId}");
        }

        public Task<string> GetExecutionStatus(Guid executionId)
        {
            throw new NotImplementedException();
        }

        public Task<PipelineExecutionRecord> CreateExecution(Pipeline pipeline)
        {
            var executionRecord = new PipelineExecutionRecord
            {
                PipelineId = pipeline.Id
            };

            _logger.LogInformation("Creating execution ({executionId}) for pipeline {pipelineId}",
                executionRecord.Id, pipeline.Id);

            executionRecord.ToBeExecuted = PipelineExecutionHelper.GetExecutionOrder(pipeline);

            Store.Add(executionRecord.Id, executionRecord);

            return Task.FromResult(executionRecord);
        }

        public async Task<IList<Block>> SelectNextBlocks(Guid executionId, Pipeline pipeline)
        {
            PipelineExecutionRecord executionRecord;
            try
            {
                executionRecord = await GetById(executionId);
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

            _logger.LogDebug("Executing {executionLevelCount} blocks from level {executionLevel}",
                nextBlocks.Count, currentLevel);

            // moving blocks from to be executed list to in execution list
            foreach (var nextBlock in nextBlocks)
            {
                _logger.LogDebug(
                    "Moving block {blockId} in execution {executionId} from status to_be_executed to in_execution",
                    nextBlock.BlockId, executionId);
                executionRecord.ToBeExecuted.Remove(nextBlock);
                nextBlock.MovedToStatusInExecutionAt = DateTime.UtcNow;
                executionRecord.InExecution.Add(nextBlock);
            }

            return nextBlocks
                .Select(b => b.Block)
                .ToList();
        }

        public async Task<bool> MarkBlockAsExecuted(Guid executionId, Guid blockId)
        {
            PipelineExecutionRecord execution;
            try
            {
                execution = await GetById(executionId);
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
                "Moving block {blockId} in execution {executionId} from status in_execution to executed",
                blockId, executionId);

            block.ExecutionCompletedAt = DateTime.UtcNow;

            execution.InExecution.Remove(block);
            execution.Executed.Add(block);

            return execution.InExecution.Count > 0;
        }
    }
}