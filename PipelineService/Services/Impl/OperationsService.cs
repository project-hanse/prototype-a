using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Helper;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
	public class OperationsService : IOperationsService
	{
		private readonly ILogger<OperationsService> _logger;
		private readonly IPipelinesDao _pipelinesDao;

		public OperationsService(
			ILogger<OperationsService> logger,
			IPipelinesDao pipelinesDao)
		{
			_logger = logger;
			_pipelinesDao = pipelinesDao;
		}

		public async Task<IList<string>> GetInputDatasetKeysForOperation(Guid pipelineId, Guid operationId)
		{
			return (await _pipelinesDao.GetOperation(operationId))?.Inputs
				.Select(ds => ds.Key)
				.ToList();
		}

		public async Task<AddOperationResponse> AddOperationToPipeline(AddOperationRequest request)
		{
			_logger.LogDebug("Adding node to pipeline for request {@AddNodeRequest}", request);

			var response = new AddOperationResponse
			{
				Success = false
			};

			if (request.PredecessorOperationIds.Count is < 0 or > 2)
			{
				_logger.LogDebug("Unexpected count of predecessor operations");
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message = "An operation must have between 0 and 2 predecessors",
					Code = "P400"
				});
				return response;
			}

			// TODO reimplement this in a general way that does can handle any number of predecessors and checks if dataset types match
			var newOperation = new Operation();
			if (request.OperationTemplate.OutputType.HasValue)
			{
				newOperation.Output.Type = request.OperationTemplate.OutputType.Value;
			}

			// Hardcoded default values for plotting
			// TODO: potentially move this to the frontend
			if (newOperation.Output.Type == DatasetType.StaticPlot)
			{
				newOperation.Output.Store = "plots";
				newOperation.Output.Key = $"{Guid.NewGuid()}.svg";
			}
			else if (newOperation.Output.Type == DatasetType.Prophet)
			{
				newOperation.Output.Store = "generic_json";
				newOperation.Output.Key = $"{Guid.NewGuid()}.prophet";
			}

			if (request.PredecessorOperationIds.Count == 0)
			{
				_logger.LogDebug("Detected no predecessor nodes");
				newOperation = new Operation
				{
					Inputs =
					{
						new Dataset
						{
							Type = DatasetType.File,
							Store = request.Options["objectBucket"],
							Key = request.Options["objectKey"]
						}
					}
				};
				await _pipelinesDao.CreateRootOperation(request.PipelineId, newOperation);
			}
			else if (request.PredecessorOperationIds.Count == 1)
			{
				_logger.LogDebug("Detected 1 predecessor operation {OperationId}", request.PredecessorOperationIds);
				var predecessor = await _pipelinesDao.GetOperation(request.PredecessorOperationIds[0]);
				if (predecessor == null)
				{
					response.StatusCode = HttpStatusCode.NotFound;
					return response;
				}

				newOperation = PipelineConstructionHelpers.Successor(predecessor, newOperation);
				await _pipelinesDao.CreateSuccessor(request.PredecessorOperationIds, newOperation);
			}
			else
			{
				_logger.LogDebug("Detected 2 predecessor nodes {@OperationIds}", request.PredecessorOperationIds);
				var predecessor1 = await _pipelinesDao.GetOperation(request.PredecessorOperationIds[0]);
				var predecessor2 = await _pipelinesDao.GetOperation(request.PredecessorOperationIds[1]);
				if (predecessor1 == default || predecessor2 == default)
				{
					response.StatusCode = HttpStatusCode.NotFound;
					return response;
				}

				newOperation = PipelineConstructionHelpers.Successor(predecessor1, predecessor2, newOperation);
				await _pipelinesDao.CreateSuccessor(request.PredecessorOperationIds, newOperation);
			}

			newOperation.PipelineId = request.PipelineId;
			newOperation.OperationId = request.OperationTemplate.OperationId;
			newOperation.OperationIdentifier = request.OperationTemplate.OperationName;
			newOperation.OperationConfiguration = request.OperationTemplate.DefaultConfig;

			await _pipelinesDao.UpdateOperation(newOperation);

			_logger.LogDebug(
				"Added node {OperationId} from operation template {OperationName} ({OperationTemplateId}) to pipeline {PipelineId}",
				newOperation.Id, newOperation.OperationIdentifier, newOperation.OperationId, request.PipelineId);

			response.OperationId = newOperation.Id;
			response.PipelineId = request.PipelineId;
			response.Success = true;
			return response;
		}

		public async Task<RemoveNodesResponse> RemoveOperationsFromPipeline(RemoveOperationsRequest request)
		{
			_logger.LogDebug("Removing operations from pipeline for request {@RemoveOperationsRequest}", request);

			var response = new RemoveNodesResponse
			{
				Success = false
			};

			foreach (var operationId in request.OperationIdsToBeRemoved)
			{
				await _pipelinesDao.DeleteOperation(operationId);
			}

			response.Success = true;
			response.PipelineId = request.PipelineId;

			_logger.LogInformation("Removed {RemovedCount} operations from pipeline {PipelineId}",
				request.OperationIdsToBeRemoved.Count, request.PipelineId);
			return response;
		}

		public async Task<Dataset> GetOutputDataset(Guid pipelineId, Guid operationId)
		{
			return (await _pipelinesDao.GetOperation(operationId)).Output;
		}

		public async Task<IDictionary<string, string>> GetConfig(Guid pipelineId, Guid nodeId)
		{
			var node = await FindOperationOrDefault(pipelineId, nodeId);
			if (node == null)
			{
				_logger.LogDebug("Node with id {NotFoundId} not found", pipelineId);
			}

			return node?.OperationConfiguration;
		}

		public async Task<bool> UpdateConfig(Guid pipelineId, Guid operationId, Dictionary<string, string> config)
		{
			var operation = await _pipelinesDao.GetOperation(operationId);
			if (operation == null)
			{
				_logger.LogDebug("Operation with id {NotFoundId} not found", operationId);
				return false;
			}

			operation.OperationConfiguration = config;

			await _pipelinesDao.UpdateOperation(operation);

			_logger.LogInformation("Updated configuration for operation {OperationId} in pipeline {PipelineId}",
				operation.Id, pipelineId);

			return true;
		}

		public async Task<Operation> FindOperationOrDefault(Guid pipelineId, Guid nodeId)
		{
			return await _pipelinesDao.GetOperation(nodeId);
		}
	}
}
