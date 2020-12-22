using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Models;
using PipelineService.Services;

namespace PipelineService.Controllers
{
    public class PipelineController : BaseController
    {
        private readonly ILogger<PipelineController> _logger;
        private readonly IPipelineService _pipelineService;

        public PipelineController(
            ILogger<PipelineController> logger,
            IPipelineService pipelineService)
        {
            _logger = logger;
            _pipelineService = pipelineService;
        }

        [HttpGet("{id}")]
        public async Task<Pipeline> GetPipeline(Guid id)
        {
            return await _pipelineService.GetPipeline(id);
        }

        [HttpGet("execute")]
        public async Task<Guid> ExecutePipeline()
        {
            var id = Guid.NewGuid();
            _logger.LogInformation("Generating random id for pipeline {pipelineId}", id);

            await _pipelineService.ExecutePipeline(id);

            return id;
        }
    }
}