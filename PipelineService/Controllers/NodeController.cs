using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PipelineService.Services;

namespace PipelineService.Controllers
{
    public class NodeController : BaseController
    {
        private readonly ILogger<NodeController> _logger;
        private readonly INodesService _nodesService;

        public NodeController(
            ILogger<NodeController> logger,
            INodesService nodesService)
        {
            _logger = logger;
            _nodesService = nodesService;
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
    }
}