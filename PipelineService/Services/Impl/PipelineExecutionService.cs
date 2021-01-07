using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

        public PipelineExecutionService(
            ILogger<PipelineExecutionService> logger,
            IPipelineService pipelineService,
            IMqttMessageService mqttMessageService)
        {
            _logger = logger;
            _pipelineService = pipelineService;
            _mqttMessageService = mqttMessageService;
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

            await EnqueueBlocks(pipeline.Root);

            return execution.Id;
        }

        private async Task EnqueueBlocks(Block block)
        {
            _logger.LogInformation("Enqueuing block ({blockId}) with operation {operation}", block.Id, block.Operation);

            await _mqttMessageService.PublishMessage($"execute/{block.PipelineId}", new SimpleBlockExecutionRequest
            {
                PipelineId = block.PipelineId,
                BlockId = block.Id,
                ExecutionId = Guid.NewGuid(), // TODO: use actual execution id
                OperationName = block.Operation,
                OperationConfiguration = block.OperationConfiguration
            });

            foreach (var blockSuccessor in block.Successors)
            {
                await EnqueueBlocks(blockSuccessor);
            }
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
            var blocks = new List<Block>();
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
            
            


            return blocks;
        }
    }
}