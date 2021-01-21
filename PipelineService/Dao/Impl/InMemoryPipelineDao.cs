using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Helper;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
    public class InMemoryPipelineDao : IPipelineDao
    {
        private readonly ILogger<IPipelineDao> _logger;
        private static readonly IDictionary<Guid, Pipeline> Store = new ConcurrentDictionary<Guid, Pipeline>();

        public InMemoryPipelineDao(
            ILogger<IPipelineDao> logger)
        {
            _logger = logger;
        }

        public Task<Pipeline> Create(Guid id)
        {
            var pipelineId = Guid.NewGuid();

            _logger.LogInformation("Creating new default pipeline with id {pipelineId}", pipelineId);

            var defaultPipeline = NewDefaultPipeline(pipelineId);

            Store.Add(pipelineId, defaultPipeline);

            return Task.FromResult(defaultPipeline);
        }

        public Task<Pipeline> Get(Guid pipelineId)
        {
            if (!Store.TryGetValue(pipelineId, out var pipeline))
            {
                throw new NotFoundException("No pipeline with given id found");
            }

            _logger.LogInformation("Loading pipeline with id {pipelineId}", pipelineId);
            return Task.FromResult(pipeline);
        }

        /// <summary>
        /// Generates a default pipeline with hardcoded dataset ids for prototyping.
        /// </summary>
        private Pipeline NewDefaultPipeline(Guid pipelineId)
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
                InputDatasetHash = HashHelper.ComputeStaticHash(cleanUp),
                Operation = "select_columns",
                OperationConfiguration = new Dictionary<string, string>
                {
                    {"0", "['Rooms', 'Bathroom', 'Landsize', 'Lattitude', 'Longtitude']"}
                }
            };

            var describe = new SimpleBlock
            {
                PipelineId = pipelineId,
                InputDatasetHash = HashHelper.ComputeStaticHash(select),
                Operation = "describe"
            };

            cleanUp.Successors.Add(select);
            select.Successors.Add(describe);

            return new Pipeline
            {
                Id = pipelineId,
                Name = "Melbourne Housing Data",
                Root = new List<Block>
                {
                    cleanUp
                }
            };
        }
    }
}