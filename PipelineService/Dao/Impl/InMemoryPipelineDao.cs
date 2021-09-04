using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Exceptions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Dao.Impl
{
    public class InMemoryPipelineDao : IPipelineDao
    {
        private readonly ILogger<InMemoryPipelineDao> _logger;
        private static readonly IDictionary<Guid, Pipeline> Store = new ConcurrentDictionary<Guid, Pipeline>();

        public InMemoryPipelineDao(ILogger<InMemoryPipelineDao> logger)
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

        public async Task<IList<Pipeline>> CreateDefaults(IList<Pipeline> pipelines = null)
        {
            var newDefaultPipelines = pipelines ?? NewDefaultPipelines();

            _logger.LogInformation("Creating {NewPipelines} new pipeline(s)", newDefaultPipelines.Count);

            foreach (var pipeline in newDefaultPipelines)
            {
                await Add(pipeline);
            }

            return newDefaultPipelines;
        }

        public Task Add(Pipeline pipeline)
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

        public Task<PipelineInfoDto> GetInfoDto(Guid pipelineId)
        {
            if (!Store.TryGetValue(pipelineId, out var pipeline))
            {
                throw new NotFoundException("No pipeline with given id found");
            }

            return Task.FromResult(MapToDto(pipeline));
        }

        private static PipelineInfoDto MapToDto(Pipeline pipeline)
        {
            return new PipelineInfoDto
            {
                Id = pipeline.Id,
                Name = pipeline.Name,
                CreatedOn = pipeline.CreatedOn
            };
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

        public async Task<IList<PipelineInfoDto>> GetDtos()
        {
            var pipelines = await Get();
            return pipelines
                .OrderByDescending(p => p.CreatedOn)
                .Select(MapToDto)
                .ToList();
        }

        private static IList<Pipeline> NewDefaultPipelines()
        {
            return new List<Pipeline>
            {
                HardcodedDefaultPipelines.MelbourneHousingPipeline(),
                HardcodedDefaultPipelines.InfluenzaInterpolation(),
                HardcodedDefaultPipelines.MelbourneHousingPipelineWithError(),
                HardcodedDefaultPipelines.ChemnitzStudentAndJobsPipeline(),
                HardcodedDefaultPipelines.SimulatedVineYieldPipeline(),
                HardcodedDefaultPipelines.ZamgWeatherPreprocessingGraz(),
                HardcodedDefaultPipelines.ZamgWeatherPreprocessingGraz(Guid.NewGuid(), 1991)
            };
        }
    }
}