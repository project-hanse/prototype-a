using System;
using System.Collections.Generic;
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
	        var node = await _pipelinesDao.GetNode(nodeId);

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
	            var predecessor = await _pipelinesDao.GetNode(request.PredecessorNodeIds[0]);
                if (predecessor == null){
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                newNode = PipelineConstructionHelpers.Successor(predecessor, new NodeSingleInput());
                await _pipelinesDao.CreateSuccessor(request.PredecessorNodeIds, (NodeSingleInput)newNode);
            }
            else
            {
                var predecessor1 = await _pipelinesDao.GetNode(request.PredecessorNodeIds[0]);
                var predecessor2 =  await _pipelinesDao.GetNode(request.PredecessorNodeIds[1]);
                if (predecessor1 == default || predecessor2 == default)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                newNode = PipelineConstructionHelpers.Successor(predecessor1, predecessor2, new NodeDoubleInput());
                await _pipelinesDao.CreateSuccessor(request.PredecessorNodeIds, (NodeDoubleInput) newNode);
            }

            newNode.PipelineId = request.PipelineId;
            newNode.OperationId = request.Operation.OperationId;
            newNode.Operation = request.Operation.OperationName;
            newNode.OperationConfiguration = request.Operation.DefaultConfig;

            await _pipelinesDao.UpdateNode(newNode);

            _logger.LogDebug(
                "Added node {NodeId} with operation {OperationName} ({OperationId}) to pipeline {PipelineId}",
                newNode.Id, newNode.Operation, newNode.OperationId, request.PipelineId);

            response.NodeId = newNode.Id;
            response.PipelineId = request.PipelineId;
            response.Success = true;
            return response;
        }

        public async Task<RemoveNodesResponse> RemoveNodesFromPipeline(RemoveNodesRequest request)
        {
            _logger.LogDebug("Removing nodes from pipeline for request {@RemoveNodesRequest}", request);

            var response = new RemoveNodesResponse
            {
                Success = false
            };

            foreach (var nodeId in request.NodeIdsToBeRemoved)
            {
	           await _pipelinesDao.DeleteNode(nodeId);
            }

            response.Success = true;
            response.PipelineId = request.PipelineId;

            _logger.LogInformation("Removed {RemovedCount} nodes from pipeline {PipelineId}",
                request.NodeIdsToBeRemoved.Count, request.PipelineId);
            return response;
        }

        public async Task<string> GetResultHash(Guid pipelineId, Guid nodeId)
        {
            return (await _pipelinesDao.GetNode(nodeId)).ResultKey;
        }

        public async Task<IDictionary<string, string>> GetConfig(Guid pipelineId, Guid nodeId)
        {
            var node = await FindNodeOrDefault(pipelineId, nodeId);
            if (node == null)
            {
                _logger.LogDebug("Node with id {NotFoundId} not found", pipelineId);
            }

            return node?.OperationConfiguration;
        }

        public async Task<bool> UpdateConfig(Guid pipelineId, Guid nodeId, Dictionary<string, string> config)
        {
	          var node = await _pipelinesDao.GetNode(nodeId);
            if (node == null)
            {
                _logger.LogDebug("Node with id {NotFoundId} not found", nodeId);
                return false;
            }

            node.OperationConfiguration = config;

            await _pipelinesDao.UpdateNode(node);

            _logger.LogInformation("Updated configuration for node {NodeId} in pipeline {PipelineId}",
                node.Id, pipelineId);

            return true;
        }

        public async Task<Node> FindNodeOrDefault(Guid pipelineId, Guid nodeId)
        {
	        return await _pipelinesDao.GetNode(nodeId);
        }
    }
}
