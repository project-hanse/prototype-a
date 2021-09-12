using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Models.Dtos;
using PipelineService.Services;

namespace PipelineService.Controllers
{
    public class NodeController : BaseController
    {
        private readonly ILogger<NodeController> _logger;
        private readonly INodesService _nodesService;
        private readonly IPipelineExecutionService _pipelineExecutionService;

        public NodeController(
            ILogger<NodeController> logger,
            INodesService nodesService,
            IPipelineExecutionService pipelineExecutionService)
        {
            _logger = logger;
            _nodesService = nodesService;
            _pipelineExecutionService = pipelineExecutionService;
        }

        [HttpGet("{pipelineId:Guid}/{nodeId:Guid}/datasets/input")]
        public async Task<IActionResult> GetInputDatasetIds(Guid pipelineId, Guid nodeId)
        {
            _logger.LogInformation("Loading input datasets for {NodeId} of pipeline {PipelineId}",
                nodeId, pipelineId);

            var result = await _nodesService.GetInputDatasetIdsForNode(pipelineId, nodeId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("{pipelineId:Guid}/{nodeId:Guid}/result-hash")]
        public async Task<IActionResult> GetResultHash(Guid pipelineId, Guid nodeId)
        {
            return Ok(new { Hash = await _nodesService.GetResultHash(pipelineId, nodeId) });
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddNode(AddNodeRequest request)
        {
            var response = await _nodesService.AddNodeToPipeline(request);

            if (!response.Success) return BadRequest(response);

            response.PipelineVisualizationDto =
                await _pipelineExecutionService.GetPipelineForVisualization(response.PipelineId);
            return Ok(response);
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveNodes(RemoveNodesRequest request)
        {
            var response = await _nodesService.RemoveNodesFromPipeline(request);

            if (!response.Success) return BadRequest(response);

            response.PipelineVisualizationDto =
                await _pipelineExecutionService.GetPipelineForVisualization(response.PipelineId);
            return Ok(response);
        }
    }
}