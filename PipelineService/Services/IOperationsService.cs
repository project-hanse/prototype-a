using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services
{
	public interface IOperationsService
	{
		public Task<IList<string>> GetInputDatasetKeysForOperation(Guid pipelineId, Guid operationId);
		public Task<AddOperationResponse> AddOperationToPipeline(AddOperationRequest request);
		public Task<RemoveNodesResponse> RemoveOperationsFromPipeline(RemoveOperationsRequest request);
		public Task<Dataset> GetOutputDataset(Guid pipelineId, Guid operationId);
		public Task<IDictionary<string, string>> GetConfig(Guid pipelineId, Guid nodeId);
		public Task<bool> UpdateConfig(Guid pipelineId, Guid operationId, Dictionary<string, string> config);
		public Task<Operation> FindOperationOrDefault(Guid pipelineId, Guid nodeId);
	}
}
