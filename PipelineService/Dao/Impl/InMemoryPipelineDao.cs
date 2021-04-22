using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
    public class InMemoryPipelineDao : IPipelineDao
    {
        private readonly ILogger<InMemoryPipelineDao> _logger;
        private static readonly IDictionary<Guid, Pipeline> Store = new ConcurrentDictionary<Guid, Pipeline>();

        public InMemoryPipelineDao(
            ILogger<InMemoryPipelineDao> logger)
        {
            _logger = logger;
        }

        public Task<Pipeline> Create(Guid id)
        {
            var pipelineId = Guid.NewGuid();

            _logger.LogInformation("Creating new default pipeline with id {PipelineId}", pipelineId);

            var defaultPipeline = HardcodedDefaultPipelines.MelbourneHousingPipeline(id);

            Store.Add(pipelineId, defaultPipeline);

            return Task.FromResult(defaultPipeline);
        }

        public async Task<IList<Pipeline>> CreateDefaults()
        {
            var pipelines = NewDefaultPipelines();

            _logger.LogInformation("Creating {NewPipelines} new pipeline(s)", pipelines.Count);

            foreach (var pipeline in pipelines)
            {
                await Add(pipeline);
            }

            return pipelines;
        }

        private Task Add(Pipeline pipeline)
        {
            if (!Store.ContainsKey(pipeline.Id))
            {
                Store.Add(pipeline.Id, pipeline);
                _logger.LogDebug("Added new pipeline with id {PipelineId}", pipeline.Id);
            }
            else
            {
                _logger.LogWarning("Pipeline with id {PipelineId} already exists", pipeline.Id);
            }

            return Task.CompletedTask;
        }

        public Task<Pipeline> Get(Guid pipelineId)
        {
            if (!Store.TryGetValue(pipelineId, out var pipeline))
            {
                throw new NotFoundException("No pipeline with given id found");
            }

            _logger.LogInformation("Loading pipeline with id {PipelineId}", pipelineId);
            return Task.FromResult(pipeline);
        }

        public async Task<IList<Pipeline>> Get()
        {
            if (Store.Count == 0)
            {
                _logger.LogInformation("Creating new default pipelines");
                await CreateDefaults();
            }

            var pipelines = Store
                .Select(r => r.Value)
                .ToList();

            _logger.LogInformation("Loading {PipelineCount} pipeline(s)", pipelines.Count);

            return pipelines;
        }

        private static IList<Pipeline> NewDefaultPipelines()
        {
            return new List<Pipeline>
            {
                HardcodedDefaultPipelines.MelbourneHousingPipeline(Guid.Parse("ab6eb2a5-093f-4467-8ce7-3c0529be272f")),
                HardcodedDefaultPipelines.InfluenzaInterpolation(Guid.Parse("c0ad2f0a-706b-44e0-b485-ac7b234abd37")),
                HardcodedDefaultPipelines.MelbourneHousingPipelineWithError(Guid.Parse("fee8bac5-0e3b-4d73-98b6-2f2e4c37afe0"))
            };
        }
    }
}