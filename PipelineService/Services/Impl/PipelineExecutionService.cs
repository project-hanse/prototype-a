using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models;

namespace PipelineService.Services.Impl
{
    public class PipelineExecutionService : IPipelineExecutionService
    {
        private readonly ILogger<PipelineExecutionService> _logger;
        private readonly IPipelineService _pipelineService;

        public PipelineExecutionService(
            ILogger<PipelineExecutionService> logger,
            IPipelineService pipelineService)
        {
            _logger = logger;
            _pipelineService = pipelineService;
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

            var execution = new PipelineExecution
            {
                PipelineId = pipeline.Id
            };

            await EnqueueBlocks(pipeline.Root);

            return execution.Id;
        }

        private async Task EnqueueBlocks(Block block)
        {
            _logger.LogInformation("Enqueuing block ({blockId}) with operation {operation}", block.Id, block.Operation);

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