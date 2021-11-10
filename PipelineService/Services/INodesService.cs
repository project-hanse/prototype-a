using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
	public interface INodesService
	{
		public Task<IList<string>> GetInputDatasetIdsForNode(Guid pipelineId, Guid nodeId);
		public Task<AddNodeResponse> AddNodeToPipeline(AddNodeRequest request);
		public Task<RemoveNodesResponse> RemoveNodesFromPipeline(RemoveNodesRequest request);
		public Task<string> GetResultHash(Guid pipelineId, Guid nodeId);
		public Task<Dictionary<string, string>> GetConfig(Guid pipelineId, Guid nodeId);
		public Task<bool> UpdateConfig(Guid pipelineId, Guid nodeId, Dictionary<string, string> config);
		public Task<Node> FindNodeOrDefault(Guid pipelineId, Guid nodeId);
	}
}
