using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;

namespace PipelineService.Services
{
    public interface INodesService
    {
        public Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId);
        public Task<AddNodeResponse> AddNodeToPipeline(AddNodeRequest request);
        public Task<RemoveNodesResponse> RemoveNodesFromPipeline(RemoveNodesRequest request);
    }
}