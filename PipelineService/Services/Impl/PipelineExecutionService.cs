using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;
using PipelineService.Models.Pipeline;
using PipelineService.Models.Pipeline.Execution;

namespace PipelineService.Services.Impl
{
    public class PipelineExecutionService : IPipelineExecutionService
    {
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

            await _mqttMessageService.PublishMessage($"execute/{block.PipelineId}", new BlockExecutionRequest
            {
                PipelineId = block.PipelineId,
                BlockId = block.Id,
                ExecutionId = Guid.NewGuid(), // TODO: use actual execution id
                OperationName = block.Operation
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
    }
}