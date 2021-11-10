using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Exceptions;
using PipelineService.Helper;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
    public class NodesService : INodesService
    {
        private readonly ILogger<NodesService> _logger;
        private readonly IPipelinesDaoInMemory _pipelinesDaoInMemory;

        public NodesService(
            ILogger<NodesService> logger,
            IPipelinesDaoInMemory pipelinesDaoInMemory)
        {
            _logger = logger;
            _pipelinesDaoInMemory = pipelinesDaoInMemory;
        }

        public async Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId)
        {
            var pipeline = await _pipelinesDaoInMemory.Get(pipelineId);

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
            var pipeline = await _pipelinesDaoInMemory.Get(request.PipelineId);

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
                var predecessor = FindNodeOrDefault(request.PredecessorNodeIds[0], pipeline);
                if (predecessor == default)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                newNode = PipelineConstructionHelpers.Successor(predecessor, new NodeSingleInput());
            }
            else
            {
                var predecessor1 = FindNodeOrDefault(request.PredecessorNodeIds[0], pipeline);
                var predecessor2 = FindNodeOrDefault(request.PredecessorNodeIds[1], pipeline);
                if (predecessor1 == default || predecessor2 == default)
                {
                    response.StatusCode = HttpStatusCode.NotFound;
                    return response;
                }

                newNode = PipelineConstructionHelpers.Successor(predecessor1, predecessor2, new NodeDoubleInput());
            }

            newNode.PipelineId = pipeline.Id;
            newNode.OperationId = request.Operation.OperationId;
            newNode.Operation = request.Operation.OperationName;
            newNode.OperationConfiguration = request.Operation.DefaultConfig;

            pipeline = await _pipelinesDaoInMemory.Update(pipeline);

            _logger.LogDebug(
                "Added node {NodeId} with operation {OperationName} ({OperationId}) to pipeline {PipelineId}",
                newNode.Id, newNode.Operation, newNode.OperationId, pipeline.Id);

            response.NodeId = newNode.Id;
            response.PipelineId = pipeline.Id;
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
            var pipeline = await _pipelinesDaoInMemory.Get(request.PipelineId);

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

            foreach (var nodeId in request.NodeIdsToBeRemoved)
            {
                RemoveRecursively(nodeId, pipeline.Root);
            }

            pipeline = await _pipelinesDaoInMemory.Update(pipeline);

            response.Success = true;
            response.PipelineId = pipeline.Id;

            _logger.LogInformation("Removed {RemovedCount} nodes from pipeline {PipelineId}",
                request.NodeIdsToBeRemoved.Count, pipeline.Id);
            return response;
        }

        public async Task<string> GetResultHash(Guid pipelineId, Guid nodeId)
        {
            return FindNodeOrDefault(nodeId, await _pipelinesDaoInMemory.Get(pipelineId)).ResultKey;
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
            var pipeline = await _pipelinesDaoInMemory.Get(pipelineId);
            if (pipeline == null)
            {
                _logger.LogDebug("Pipeline with id {NotFoundId} not found", pipelineId);
                return false;
            }

            var node = FindNodeOrDefault(nodeId, pipeline);
            if (node == null)
            {
                _logger.LogDebug("Node with id {NotFoundId} not found", nodeId);
                return false;
            }

            node.OperationConfiguration = config;
            pipeline = await _pipelinesDaoInMemory.Update(pipeline);

            _logger.LogInformation("Updated configuration for node {NodeId} in pipeline {PipelineId}",
                node.Id, pipeline.Id);

            return true;
        }

        private static void RemoveRecursively(Guid nodeId, IList<Node> nodes)
        {
            for (var i = 0; i < nodes.Count; i++)
            {
                if (nodes[i].Id == nodeId)
                {
                    nodes.RemoveAt(i);
                    return;
                }

                RemoveRecursively(nodeId, nodes[i].Successors);
            }
        }

        public async Task<Node> FindNodeOrDefault(Guid pipelineId, Guid nodeId)
        {
            Pipeline pipeline;
            try
            {
                pipeline = await _pipelinesDaoInMemory.Get(pipelineId);
                if (pipeline == null)
                {
                    _logger.LogDebug("Pipeline with id {NotFoundId} not found", pipelineId);
                }
            }
            catch (NotFoundException e)
            {
                _logger.LogDebug("Pipeline with id {NotFoundId} not found - {ErrorMessage}", pipelineId, e.Message);
                return null;
            }

            return pipeline == null ? default : FindNodeOrDefault(nodeId, pipeline.Root);
        }

        private Node FindNodeOrDefault(Guid nodeId, Pipeline pipeline)
        {
            var node = FindNodeOrDefault(nodeId, pipeline.Root);
            if (node == default)
            {
                _logger.LogInformation("Could not find node {NotFoundId} in pipeline {PipelineId}",
                    nodeId, pipeline.Id);
            }

            return node;
        }

        private static Node FindNodeOrDefault(Guid nodeId, IList<Node> nodes)
        {
            foreach (var node in nodes)
            {
                if (node.Id == nodeId)
                {
                    return node;
                }

                var n = FindNodeOrDefault(nodeId, node.Successors);
                if (n != default)
                {
                    return n;
                }
            }

            return default;
        }
    }
}
