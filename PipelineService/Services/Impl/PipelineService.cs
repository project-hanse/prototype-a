using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models;
using PipelineService.Models.Pipeline;

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

            var defaultPipeline = NewDefaultPipeline(pipelineId);

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

        /// <summary>
        /// Generates a default pipeline with hardcoded dataset ids for prototyping.
        /// </summary>
        private static Pipeline NewDefaultPipeline(Guid pipelineId)
        {
            var cleanUp = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetId = Guid.Parse("00e61417-cada-46db-adf3-a5fc89a3b6ee"),
                Operation = "dropna",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"axis", "0"}
                },
            };

            var select = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = cleanUp.ComputeProducingHash(),
                Operation = "select_columns",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['Rooms', 'Bathroom', 'Landsize', 'Lattitude', 'Longtitude']"}
                }
            };

            var describe = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = select.ComputeProducingHash(),
                Operation = "describe"
            };

            cleanUp.Successors.Add(select);
            select.Successors.Add(describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = cleanUp
            };
        }
    }
}