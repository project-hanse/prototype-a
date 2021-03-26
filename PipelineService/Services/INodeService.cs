using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PipelineService.Services
{
    public interface INodeService
    {
        public Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId);
    }
}