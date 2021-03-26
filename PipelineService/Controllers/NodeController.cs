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
        private readonly INodeService _nodeService;

        public NodeController(
            ILogger<NodeController> logger,
            INodeService nodeService)
        {
            _logger = logger;
            _nodeService = nodeService;
        }

        [HttpGet("{pipelineId}/{nodeId}/datasets/input")]
        public async Task<IActionResult> GetInputDatasetIds(Guid pipelineId, Guid nodeId)
        {
            _logger.LogInformation("Loading input datasets for {NodeId} of pipeline {PipelineId}",
                nodeId, pipelineId);

            var result = await _nodeService.GetInputDatasetIdsForNode(pipelineId, nodeId);

            if (result == null)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}