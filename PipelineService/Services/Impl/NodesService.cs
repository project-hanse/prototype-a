using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class NodesService : INodesService
    {
        private readonly ILogger<NodesService> _logger;
        private readonly IPipelinesDao _pipelinesDao;

        public NodesService(
            ILogger<NodesService> logger,
            IPipelinesDao pipelinesDao)
        {
            _logger = logger;
            _pipelinesDao = pipelinesDao;
        }

        public async Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId)
        {
            var pipeline = await _pipelinesDao.Get(pipelineId);

            if (pipeline == null)
            {
                _logger.LogDebug("Cannot load node of unknown pipeline");
                return null;
            }

            var node = FindNodeOrDefault(nodeId, pipeline.Root);

            if (node == null)
            {
                return null;
            }

            var ids = new List<string>();

            if (node is NodeSingleInput sn)
            {
                ids.Add(sn.InputDatasetId.HasValue ? sn.InputDatasetId.Value.ToString() : sn.InputDatasetHash);
            }

            return ids;
        }

        private static Node FindNodeOrDefault(Guid nodeId, IList<Node> nodes)
        {
            var node = nodes.FirstOrDefault(n => n.Id == nodeId);

            return node != null
                ? node
                : nodes.Select(block => FindNodeOrDefault(nodeId, block.Successors)).FirstOrDefault();
        }
    }
}