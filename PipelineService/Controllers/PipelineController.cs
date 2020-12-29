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
        private readonly IPipelineExecutionService _pipelineExecutionService;

        public PipelineController(
            ILogger<PipelineController> logger,
            IPipelineService pipelineService,
            IPipelineExecutionService pipelineExecutionService)
        {
            _logger = logger;
            _pipelineService = pipelineService;
            _pipelineExecutionService = pipelineExecutionService;
        }

        [HttpGet("default")]
        public async Task<Pipeline> GetDefault()
        {
            return await _pipelineService.CreateDefault();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPipeline(Guid id)
        {
            var pipeline = await _pipelineService.GetPipeline(id);

            if (pipeline == null)
            {
                return NotFound($"No pipeline with id {id} exists");
            }

            return Ok(pipeline);
        }

        [HttpGet("execute/{pipelineId}")]
        public async Task<IActionResult> ExecutePipeline(Guid pipelineId)
        {
            _logger.LogDebug("Executing pipeline {pipelineId}", pipelineId);

            var executionId = await _pipelineExecutionService.ExecutePipeline(pipelineId);

            _logger.LogInformation("Execution of pipeline ({pipelineId}) with execution id {executionId} started",
                pipelineId, executionId);

            if (executionId.HasValue)
            {
                return Ok(executionId.Value);
            }

            return NotFound($"No pipeline with id {pipelineId} exists");
        }
    }
}