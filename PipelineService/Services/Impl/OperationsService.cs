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
		private readonly IOperationTemplatesService _operationTemplatesService;

		public OperationsService(
			ILogger<OperationsService> logger,
			IPipelinesDao pipelinesDao,
			IOperationTemplatesService operationTemplatesService)
		{
			_logger = logger;
			_pipelinesDao = pipelinesDao;
			_operationTemplatesService = operationTemplatesService;
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

			_logger.LogDebug("Validating predecessor counts");
			if (request.PredecessorOperationIds.Count != 0 &&
			    request.PredecessorOperationIds.Count != request.OperationTemplate.InputTypes.Count)
			{
				_logger.LogDebug(
					"Unexpected amount of predecessor operations. Got {ReceivedPredecessorCount}, expected {ExpectedPredecessorCount}",
					request.PredecessorOperationIds.Count, request.OperationTemplate.InputTypes.Count);
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message = $"This operation requires {request.OperationTemplate.InputTypes.Count} predecessor(s)",
					Code = "P400"
				});
				return response;
			}

			_logger.LogDebug("Validating predecessor types");
			var predecessorTypes = (await _pipelinesDao.GetOutputDatasets(request.PredecessorOperationIds))
				.Select(ds => ds.Type).ToList();

			for (var i = 0; i < predecessorTypes.Count; i++)
			{
				if (predecessorTypes[i] == request.OperationTemplate.InputTypes[i]) continue;
				_logger.LogDebug(
					"Unexpected predecessor type. Got {ReceivedPredecessorType}, expected {ExpectedPredecessorType}",
					predecessorTypes[i], request.OperationTemplate.InputTypes[i]);
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message =
						$"This operation requires the following types: {string.Join(", ", request.OperationTemplate.InputTypes)}",
					Code = "P400"
				});
				return response;
			}

			// TODO reimplement this in a general way that does can handle any number of predecessors and checks if dataset types match
			var newOperation = new Operation();
			if (request.OperationTemplate.OutputTypes.Count > 1)
			{
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message = $"Operations with more than 1 output type are not supported yet",
					Code = "P400"
				});
				return response;
			}

			newOperation.Outputs = new List<Dataset> {new()};
			if (request.OperationTemplate.OutputTypes[0].HasValue)
			{
				newOperation.Outputs[0].Type = request.OperationTemplate.OutputTypes[0].Value;
			}

			// Hardcoded default values for plotting
			// TODO: potentially move this to the frontend
			if (newOperation.Outputs[0].Type == DatasetType.StaticPlot)
			{
				newOperation.Outputs[0].Store = "plots";
				newOperation.Outputs[0].Key = $"{Guid.NewGuid()}.svg";
			}
			else if (newOperation.Outputs[0].Type == DatasetType.Prophet)
			{
				newOperation.Outputs[0].Store = "generic_json";
				newOperation.Outputs[0].Key = $"{Guid.NewGuid()}.prophet";
			}

			if (request.PredecessorOperationIds.Count == 0)
			{
				_logger.LogDebug("Detected no predecessor nodes");
				newOperation.Inputs.Clear();
				newOperation.Inputs.Add(new Dataset
				{
					Type = DatasetType.File,
					Store = request.Options["objectBucket"],
					Key = request.Options["objectKey"]
				});
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
				_logger.LogDebug("Detected {PredecessorCount} predecessor nodes {@OperationIds}",
					request.PredecessorOperationIds.Count, request.PredecessorOperationIds);
				foreach (var predecessorOperationId in request.PredecessorOperationIds)
				{
					var predecessor = await _pipelinesDao.GetOperation(predecessorOperationId);
					if (predecessor == default)
					{
						_logger.LogInformation("Predecessor operation {PredecessorOperationId} not found",
							predecessorOperationId);
						response.StatusCode = HttpStatusCode.NotFound;
						return response;
					}

					newOperation = PipelineConstructionHelpers.Successor(predecessor, newOperation);
				}

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

		public async Task<IList<Dataset>> GetOutputDatasets(Guid pipelineId, Guid operationId)
		{
			return (await _pipelinesDao.GetOperation(operationId)).Outputs;
		}

		public async Task<IDictionary<string, string>> GetConfig(Guid pipelineId, Guid nodeId)
		{
			var node = await FindOperationOrDefault(pipelineId, nodeId);
			if (node == null)
			{
				_logger.LogDebug("Node with id {NotFoundId} not found", pipelineId);
			}

			var config = node?.OperationConfiguration ?? new Dictionary<string, string>();

			if (config.Count != 0) return config;

			_logger.LogDebug("Node {NodeId} has no configuration", nodeId);
			if (node == null) return config;

			_logger.LogDebug("Loading default configuration for operation {OperationId}", node.OperationId);
			var template = await _operationTemplatesService.GetTemplate(node.OperationId, node.OperationIdentifier);
			if (template != null)
			{
				_logger.LogInformation("Falling back to default configuration for operation {OperationId}",
					node.OperationId);
				config = template.DefaultConfig;
			}

			return config;
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
