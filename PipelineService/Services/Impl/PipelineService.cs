using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models;

namespace PipelineService.Services.Impl
{
    public class PipelineService : IPipelineService
    {
        private readonly IDictionary<Guid, Pipeline> _store;

        public PipelineService()
        {
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

            return pipeline;
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
                                    Operation = "Cluster"
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