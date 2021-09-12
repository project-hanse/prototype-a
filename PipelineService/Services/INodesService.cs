using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineService.Services
{
    public interface INodesService
    {
        public Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId);
    }
}