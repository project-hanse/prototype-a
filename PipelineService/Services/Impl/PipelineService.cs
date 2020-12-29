using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models;

namespace PipelineService.Services.Impl
{
    public class PipelineService : IPipelineService
    {
        private readonly ILogger<IPipelineService> _logger;
        private static readonly IDictionary<Guid, Pipeline> Store = new ConcurrentDictionary<Guid, Pipeline>();

        public PipelineService(ILogger<IPipelineService> logger)
        {
            _logger = logger;
        }

        public Task<Pipeline> CreateDefault()
        {
            var pipelineId = Guid.NewGuid();

            _logger.LogInformation("Creating new default pipeline with id {pipelineId}", pipelineId);

            var defaultPipeline = NewPipeline(pipelineId);

            Store.Add(pipelineId, defaultPipeline);

            return Task.FromResult(defaultPipeline);
        }

        public Task<Pipeline> GetPipeline(Guid pipelineId)
        {
            if (Store.TryGetValue(pipelineId, out var pipeline))
            {
                _logger.LogInformation("Loading pipeline with id {pipelineId}", pipelineId);
                return Task.FromResult(pipeline);
            }

            _logger.LogInformation("No pipeline with id {pipelineId} found", pipelineId);
            return Task.FromResult<Pipeline>(null);
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