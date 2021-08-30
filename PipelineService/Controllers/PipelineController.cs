using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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

        [HttpGet]
        public async Task<IActionResult> GetPipelineDtos()
        {
            return Ok(await _pipelineExecutionService.GetPipelineDtos());
        }

        [HttpGet("{pipelineId:Guid}")]
        public async Task<IActionResult> GetPipelineDto(Guid pipelineId)
        {
            var pipelineDto = await _pipelineExecutionService.GetPipelineInfoDto(pipelineId);

            if (pipelineDto == null)
            {
                return NotFound($"No pipeline with id {pipelineId} exists");
            }

            return Ok(pipelineDto);
        }

        [HttpGet("vis/{pipelineId:Guid}")]
        public async Task<IActionResult> GetPipelineForVisualization(Guid pipelineId)
        {
            var pipelineVisDto = await _pipelineExecutionService.GetPipelineForVisualization(pipelineId);
            if (pipelineVisDto == null)
            {
                return NotFound();
            }

            return Ok(pipelineVisDto);
        }

        [HttpGet("execute/{pipelineId:Guid}")]
        public async Task<IActionResult> ExecutePipeline(Guid pipelineId)
        {
            _logger.LogInformation("Executing pipeline {PipelineId}", pipelineId);

            var execution = await _pipelineExecutionService.ExecutePipeline(pipelineId);

            _logger.LogDebug("Execution of pipeline ({PipelineId}) with execution id {ExecutionId} started",
                pipelineId, execution);

            return Ok(execution);
        }
    }
}