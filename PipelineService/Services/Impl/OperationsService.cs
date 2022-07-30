using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Extensions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
	public class OperationsService : IOperationsService
	{
		private readonly ILogger<OperationsService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IMemoryCache _cache;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IOperationTemplatesService _operationTemplatesService;
		private readonly IDatasetServiceClient _datasetServiceClient;

		public string OperationConfigOptions => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("OperationConfigOptions", Path.Combine("Resources", "OperationConfigOptions")));

		public string OperationConfigOptionsSharedFile => Path.Combine(OperationConfigOptions, "shared.json");
		public string OperationConfigOptionsFolder => Path.Combine(OperationConfigOptions, "OperationSpecific");

		public OperationsService(
			ILogger<OperationsService> logger,
			IConfiguration configuration,
			IMemoryCache cache,
			IPipelinesDao pipelinesDao,
			IOperationTemplatesService operationTemplatesService,
			IDatasetServiceClient datasetServiceClient)
		{
			_logger = logger;
			_configuration = configuration;
			_cache = cache;
			_pipelinesDao = pipelinesDao;
			_operationTemplatesService = operationTemplatesService;
			_datasetServiceClient = datasetServiceClient;
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

			var newOperationTemplate = await _operationTemplatesService.GetTemplate(
				request.NewOperationTemplate.OperationId, request.NewOperationTemplate.OperationName);

			if (newOperationTemplate == null)
			{
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message =
						$"This operation template ({request.NewOperationTemplate.OperationId} / {request.NewOperationTemplate.OperationName}) is not found",
					Code = "O404"
				});
				return response;
			}

			var operationInputVector = new List<OperationInputValue>();
			foreach (var predecessorOperationDto in request.PredecessorOperationDtos)
			{
				foreach (var outputDataset in predecessorOperationDto.OutputDatasets)
				{
					operationInputVector.Add(new OperationInputValue
					{
						PredecessorOperationId = predecessorOperationDto.OperationId,
						PredecessorOperationTemplateId = predecessorOperationDto.OperationTemplateId,
						OutputDataset = outputDataset
					});
				}
			}

			_logger.LogDebug("New operation input vector: {@OperationInputVector}", operationInputVector);

			if (operationInputVector.Count != newOperationTemplate.InputTypes.Count)
			{
				response.StatusCode = HttpStatusCode.BadRequest;
				response.Errors.Add(new Error
				{
					Message =
						$"The number of provided datasets ({operationInputVector.Count}) does not match the number of input types ({newOperationTemplate.InputTypes.Count})",
					Code = "O400"
				});
				_logger.LogInformation(
					"The number of provided datasets ({ProvidedDatasetCount}) does not match the number of input types ({ExpectedDatasetCount})",
					operationInputVector.Count, newOperationTemplate.InputTypes.Count);
				return response;
			}

			var newOperation = new Operation
			{
				CreatedOn = DateTime.UtcNow,
				PipelineId = request.PipelineId,
				OperationId = newOperationTemplate.OperationId,
				OperationIdentifier = newOperationTemplate.OperationName,
				OperationConfiguration = newOperationTemplate.DefaultConfig,
			};

			// merge newOperation.OperationConfiguration with request.Options
			try
			{
				newOperation.OperationConfiguration.AddAll(request.Options);
			}
			catch (Exception e)
			{
				_logger.LogWarning("Failure to merge operation configuration with options: {@Exception}", e);
			}

			if (operationInputVector.All(v => !v.PredecessorOperationId.HasValue))
			{
				_logger.LogInformation("Detected no predecessor nodes - creating new root operation");
				newOperation.Inputs.Clear();
				if (request.Options.ContainsKey("objectBucket") && request.Options.ContainsKey("objectKey"))
				{
					_logger.LogInformation("Detected new file as root operation");
					newOperation.Inputs.Clear();
					newOperation.Inputs.Add(new Dataset
					{
						Type = DatasetType.File,
						Store = request.Options["objectBucket"],
						Key = request.Options["objectKey"]
					});
				}

				await _pipelinesDao.CreateRootOperation(request.PipelineId, newOperation);
			}
			else
			{
				_logger.LogInformation("Detected {PredecessorCount} predecessor nodes - creating new operation",
					operationInputVector.Count);
				_logger.LogDebug("Verifying provided dataset types with operation template input types...");
				var predecessorIds = new List<Guid>();
				for (var i = 0; i < newOperationTemplate.InputTypes.Count; i++)
				{
					if (operationInputVector[i].OutputDataset.Type != newOperationTemplate.InputTypes[i])
					{
						_logger.LogInformation(
							"Provided dataset type ({ProvidedDatasetType}) does not match operation template input type ({ExpectedDatasetType})",
							operationInputVector[i].OutputDataset.Type, newOperationTemplate.InputTypes[i]);
						response.StatusCode = HttpStatusCode.BadRequest;
						response.Errors.Add(new Error
						{
							Message =
								$"Provided dataset type ({operationInputVector[i].OutputDataset.Type}) does not match operation template input type ({newOperationTemplate.InputTypes[i]})",
							Code = "O400"
						});
						return response;
					}

					if (!operationInputVector[i].PredecessorOperationId.HasValue)
					{
						response.StatusCode = HttpStatusCode.NotFound;
						_logger.LogInformation("Predecessor operation requires an id");
						return response;
					}

					var predecessor = await _pipelinesDao.GetOperation(operationInputVector[i].PredecessorOperationId.Value);
					if (predecessor == null)
					{
						response.StatusCode = HttpStatusCode.NotFound;
						_logger.LogInformation("Predecessor operation {PredecessorId} not found",
							operationInputVector[i].PredecessorOperationId);
						return response;
					}

					newOperation.Inputs.Add(new Dataset
					{
						Key = operationInputVector[i].OutputDataset.Key,
						Type = operationInputVector[i].OutputDataset.Type,
						Store = operationInputVector[i].OutputDataset.Store
					});
					predecessorIds.Add(operationInputVector[i].PredecessorOperationId.Value);
				}

				_logger.LogDebug("Storing new operation {OperationId}...", newOperation.Id);
				await _pipelinesDao.CreateSuccessor(predecessorIds, newOperation);
			}

			_logger.LogDebug("Creating output datasets for new operation...");

			newOperation.Outputs = new List<Dataset>();
			foreach (var datasetType in request.NewOperationTemplate.OutputTypes)
			{
				if (!datasetType.HasValue) continue;
				var newOutput = new Dataset
				{
					Type = datasetType.Value
				};

				// Hardcoded default values for plotting
				// TODO: potentially move this to the frontend
				if (newOutput.Type == DatasetType.StaticPlot)
				{
					newOutput.Key = $"{Guid.NewGuid()}.svg";
				}
				else if (newOutput.Type == DatasetType.Prophet)
				{
					newOutput.Key = $"{Guid.NewGuid()}.prophet";
				}

				newOutput.Store = GetStore(datasetType);

				newOperation.Outputs.Add(newOutput);
			}

			await _pipelinesDao.UpdateOperation(newOperation);

			_logger.LogInformation(
				"Added operation {OperationId} from operation template {OperationName} ({OperationTemplateId}) to pipeline {PipelineId}",
				newOperation.Id, newOperation.OperationIdentifier, newOperation.OperationId, request.PipelineId);

			response.OperationId = newOperation.Id;
			response.PipelineId = request.PipelineId;
			response.Success = true;
			response.ResultingDatasets = newOperation.Outputs;
			return response;
		}

		private static string GetStore(DatasetType? datasetType)
		{
			switch (datasetType)
			{
				case DatasetType.StaticPlot:
					return "plots";
				case DatasetType.Prophet:
					return "generic_json";
				case DatasetType.PdSeries:
					return "dataframes";
				case DatasetType.PdDataFrame:
					return "dataframes";
				case DatasetType.SklearnModel:
					return "generic_json";
				default:
					return "default";
			}
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

		public async Task<bool> UpdateConfig(Guid pipelineId, Guid operationId, IDictionary<string, string> config)
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

		public async Task<IDictionary<string, string>> GenerateRandomizedConfig(Guid operationId)
		{
			_logger.LogDebug("Generating randomized configuration for operation {OperationId}", operationId);
			var operation = await _pipelinesDao.GetOperation(operationId);
			if (operation == null)
			{
				_logger.LogInformation("Operation with id {NotFoundId} not found", operationId);
				return null;
			}

			var random = new Random();

			// load dictionary from OperationConfigOptionsSharedFile json file
			var sharedConfigurationOptions = await GetSharedOperationConfigOptions();
			var configurationOptions = await GetOperationConfigOptions(operationId);
			var template = await _operationTemplatesService.GetTemplate(operation.OperationId, operation.OperationIdentifier);
			var operationConfig = operation.OperationConfiguration;
			var newConfig = new Dictionary<string, string>();
			foreach (var (key, value) in template.DefaultConfig)
			{
				if (value == null)
				{
					// cannot guess a random value for this parameter
					continue;
				}

				if (configurationOptions.ContainsKey(key))
				{
					// operation specific configuration options
					var possibleValues = configurationOptions[key];
					var randomValue = possibleValues[random.Next(0, possibleValues.Length)];
					newConfig.Add(key, randomValue);
				}
				else if (sharedConfigurationOptions.ContainsKey(key))
				{
					// shared configuration options
					var possibleValues = sharedConfigurationOptions[key];
					var randomValue = possibleValues[random.Next(0, possibleValues.Length)];
					newConfig.Add(key, randomValue);
				}
				else if (key.Contains("col"))
				{
					// match for configuration keys that deal with columns
					foreach (var operationInput in operation.Inputs)
					{
						var metadata = await _datasetServiceClient.GetCompactMetadata(operationInput);
						if (metadata?.Columns.Length > 0)
						{
							if (value.Contains('['))
							{
								// random subset of columns
								var randomColumnsCount = random.Next(1, metadata.Columns.Length);
								var columns = metadata.Columns.OrderBy(c => Guid.NewGuid()).Take(randomColumnsCount).ToArray();
								newConfig.Add(key, JsonConvert.SerializeObject(columns));
							}
							else
							{
								// random column
								var randomColumnIndex = random.Next(0, metadata.Columns.Length);
								newConfig.Add(key, metadata.Columns[randomColumnIndex]);
							}
						}
					}
				}
				else if (int.TryParse(value, out var intValue))
				{
					// match configuration keys that have a numeric integer value
					if (operationConfig.ContainsKey(key) &&
					    int.TryParse(operationConfig[key], out var currentOperationConfigValue))
					{
						intValue = (intValue + currentOperationConfigValue) / 2;
					}

					var sign = intValue < 0 ? -1 : 1;
					// get a new randomized value from a normal distribution with mean intValue and standard deviation of intValue / 2
					var randomizedIntValue = Math.Abs((int)Math.Round(random.NextGaussian(intValue, intValue))) * sign;
					newConfig.Add(key, randomizedIntValue.ToString(CultureInfo.InvariantCulture));
				}
				// match configuration keys that have a numeric double value
				else if (float.TryParse(value.Replace(".", ","), out var floatValue))
				{
					if (operationConfig.ContainsKey(key) && float.TryParse(operationConfig[key]?.Replace(".", ","),
						    out var currentOperationConfigValue))
					{
						floatValue = (floatValue + currentOperationConfigValue) / 2;
					}

					if (floatValue >= 0 && floatValue <= 1)
					{
						// detected probability
						// generate random float between 0 and 1
						var randomFloat = random.NextDouble();
						newConfig.Add(key, Math.Round(randomFloat, 4).ToString(CultureInfo.InvariantCulture));
					}
					else
					{
						var sign = floatValue < 0 ? -1 : 1;
						// get a new randomized value from a normal distribution with mean floatValue and standard deviation of floatValue / 2
						var randomizedFloatValue = Math.Abs(Math.Round(random.NextGaussian(floatValue), 4)) * sign;
						newConfig.Add(key, randomizedFloatValue.ToString(CultureInfo.InvariantCulture));
					}
				}
				// match configuration keys that have a boolean value
				else if (value.Equals("true", StringComparison.InvariantCultureIgnoreCase) ||
				         value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
				{
					newConfig.Add(key, random.Next(0, 2) == 0 ? "true" : "false");
				}

				if (!newConfig.ContainsKey(key))
				{
					_logger.LogDebug("No new config value generated, falling back to default value");
					newConfig.Add(key, value);
				}
			}

			return newConfig;
		}

		private async Task<IDictionary<string, string[]>> GetSharedOperationConfigOptions()
		{
			if (_cache.TryGetValue<IDictionary<string, string[]>>("sharedOperationConfigOptions",
				    out var sharedOperationConfigOptions))
			{
				return sharedOperationConfigOptions;
			}

			sharedOperationConfigOptions =
				JsonConvert.DeserializeObject<IDictionary<string, string[]>>(
					await File.ReadAllTextAsync(OperationConfigOptionsSharedFile)) ?? new Dictionary<string, string[]>();

			_logger.LogInformation("Loaded shared operation config options from file");
			_cache.Set("sharedOperationConfigOptions", sharedOperationConfigOptions, TimeSpan.FromHours(1));

			return sharedOperationConfigOptions;
		}

		private async Task<IDictionary<string, string[]>> GetOperationConfigOptions(Guid operationId)
		{
			if (_cache.TryGetValue<IDictionary<string, string[]>>($"operationConfigOptions_{operationId}",
				    out var operationConfigOptions))
			{
				return operationConfigOptions;
			}

			var path = Path.Combine(OperationConfigOptionsFolder, $"{operationId}.json");

			if (!File.Exists(path))
			{
				operationConfigOptions = new Dictionary<string, string[]>();
				_logger.LogInformation("No operation config options file found for operation {OperationId}", operationId);
			}
			else
			{
				operationConfigOptions =
					JsonConvert.DeserializeObject<IDictionary<string, string[]>>(await File.ReadAllTextAsync(path));
				_logger.LogInformation("Loaded operation config options ({OperationId}) from file", operationId);
			}

			_cache.Set($"operationConfigOptions_{operationId}", operationConfigOptions, TimeSpan.FromHours(1));
			return operationConfigOptions;
		}

		public async Task<Operation> FindOperationOrDefault(Guid pipelineId, Guid nodeId)
		{
			return await _pipelinesDao.GetOperation(nodeId);
		}
	}

	public class OperationInputValue
	{
		public Guid? PredecessorOperationId { get; set; }
		public Guid? PredecessorOperationTemplateId { get; set; }
		public Dataset OutputDataset { get; set; }
	}
}
