using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models;

namespace PipelineService.Services.Impl
{
    public class PipelineService : IPipelineService
    {
        private readonly ILogger<IPipelineService> _logger;
        private readonly IDictionary<Guid, Pipeline> _store;

        public PipelineService(ILogger<IPipelineService> logger)
        {
            _logger = logger;
            _store = new ConcurrentDictionary<Guid, Pipeline>();
        }

        public async Task<Pipeline> GetPipeline(Guid pipelineId)
        {
            if (_store.TryGetValue(pipelineId, out var pipeline))
            {
                return pipeline;
            }

            pipeline = NewPipeline(pipelineId);

            _store.Add(pipelineId, pipeline);

            return pipeline;
        }

        public async Task<Pipeline> ExecutePipeline(Guid pipelineId)
        {
            var pipeline = await GetPipeline(pipelineId);

            await EnqueueBlocks(pipeline.Root);

            return pipeline;
        }

        private async Task EnqueueBlocks(Block block)
        {
            _logger.LogInformation("Enqueuing block ({blockId}) with operation {operation}", block.Id, block.Operation);

            foreach (var blockSuccessor in block.Successors)
            {
                await EnqueueBlocks(blockSuccessor);
            }
        }

        private static Pipeline NewPipeline(Guid pipelineId)
        {
            var pipeline = new Pipeline
            {
                Id = pipelineId,
                Root = new Block
                {
                    Operation = "LoadCSV",
                    Successors = new List<Block>
                    {
                        new()
                        {
                            Operation = "CleanUp",
                            Successors = new List<Block>
                            {
                                new()
                                {
                                    Operation = "Cluster",
                                    Successors = new List<Block>
                                    {
                                        new()
                                        {
                                            Operation = "Visualize"
                                        }
                                    }
                                },
                                new()
                                {
                                    Operation = "Filter",
                                    Successors = new List<Block>
                                    {
                                        new()
                                        {
                                            Operation = "Visualize"
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
            return pipeline;
        }
    }
}