using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Helper;
using PipelineService.Models.Dtos;
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

        public async Task<AddNodeResponse> AddNodeToPipeline(AddNodeRequest request)
        {
            _logger.LogDebug("Adding node to pipeline for request {@AddNodeRequest}", request);

            var response = new AddNodeResponse
            {
                Success = false
            };
            var pipeline = await _pipelinesDao.Get(request.PipelineId);

            if (pipeline == null)
            {
                _logger.LogDebug("Cannot add node to unavailable pipeline");
                response.StatusCode = HttpStatusCode.NotFound;
                response.Errors.Add(new Error
                {
                    Message = "Pipeline not found",
                    Code = "P404"
                });
                return response;
            }

            // TODO: verify that operation id exists

            if (request.PredecessorNodeIds.Count is < 1 or > 2)
            {
                _logger.LogDebug("Unexpected count of predecessor nodes");
                response.StatusCode = HttpStatusCode.BadRequest;
                response.Errors.Add(new Error
                {
                    Message = "A node must have either 0 or 2 predecessors",
                    Code = "P400"
                });
                return response;
            }

            Node newNode;
            if (request.PredecessorNodeIds.Count == 1)
            {
                var predecessor = FindNodeOrDefault(request.PredecessorNodeIds[0], pipeline.Root);
                newNode = PipelineConstructionHelpers.Successor(predecessor, new NodeSingleInput());
            }
            else
            {
                var predecessor1 = FindNodeOrDefault(request.PredecessorNodeIds[0], pipeline.Root);
                var predecessor2 = FindNodeOrDefault(request.PredecessorNodeIds[1], pipeline.Root);
                newNode = PipelineConstructionHelpers.Successor(predecessor1, predecessor2, new NodeDoubleInput());
            }

            newNode.PipelineId = pipeline.Id;
            newNode.OperationId = request.Operation.OperationId;
            newNode.Operation = request.Operation.OperationName;

            pipeline = await _pipelinesDao.Update(pipeline);

            _logger.LogDebug(
                "Added node {NodeId} with operation {OperationName} ({OperationId}) to pipeline {PipelineId}",
                newNode.Id, newNode.Operation, newNode.OperationId, pipeline.Id);

            response.NodeId = newNode.Id;
            response.PipelineId = pipeline.Id;
            response.Success = true;
            return response;
        }

        private static Node FindNodeOrDefault(Guid nodeId, IList<Node> nodes)
        {
            var node = nodes.FirstOrDefault(n => n.Id == nodeId);

            return node ?? nodes.Select(block => FindNodeOrDefault(nodeId, block.Successors)).FirstOrDefault();
        }
    }
}