using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Models.Pipeline;
using PipelineService.Services;

namespace PipelineService.Controllers
{
    public class PipelineController : BaseController
    {
        private readonly ILogger<PipelineController> _logger;
        private readonly IPipelineExecutionService _pipelineExecutionService;

        public PipelineController(
            ILogger<PipelineController> logger,
            IPipelineExecutionService pipelineExecutionService)
        {
            _logger = logger;
            _pipelineExecutionService = pipelineExecutionService;
        }

        [HttpGet("create/defaults")]
        public async Task<int> GetDefaults()
        {
            return (await _pipelineExecutionService.CreateDefaultPipelines()).Count;
        }

        [HttpGet("")]
        public async Task<IActionResult> GetPipelines()
        {
            return Ok(await _pipelineExecutionService.GetPipelines());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPipeline(Guid id)
        {
            var pipeline = await _pipelineExecutionService.GetPipeline(id);

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

            var execution = await _pipelineExecutionService.ExecutePipeline(pipelineId);

            _logger.LogInformation("Execution of pipeline ({pipelineId}) with execution id {executionId} started",
                pipelineId, execution);

            return Ok(execution);
        }
    }
}