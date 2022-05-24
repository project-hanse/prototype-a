using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Extensions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
	public class PipelinesDtoService : IPipelinesDtoService
	{
		private readonly ILogger<PipelinesDtoService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IOperationsService _operationsService;
		private readonly IHttpClientFactory _httpClientFactory;

		public string DefaultPipelinesPath => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("DefaultPipelinesFolder", ""));

		public string PipelineCandidatesPath => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("PipelineCandidatesFolder", ""));

		public PipelinesDtoService(ILogger<PipelinesDtoService> logger,
			IConfiguration configuration,
			IPipelinesDao pipelinesDao,
			IOperationsService operationsService,
			IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_configuration = configuration;
			_pipelinesDao = pipelinesDao;
			_operationsService = operationsService;
			_httpClientFactory = httpClientFactory;
		}

		public async Task<IList<OperationTuples>> GetOperationTuples()
		{
			// TODO: check if pipeline has been successfully executed
			return await _pipelinesDao.GetOperationTuples();
		}

		public async Task<PipelineExport> ExportPipeline(Guid pipelineId)
		{
			_logger.LogDebug("Exporting pipeline {PipelineId}", pipelineId);
			var infoDto = await _pipelinesDao.GetInfoDto(pipelineId);
			if (infoDto == null)
			{
				_logger.LogInformation("Pipeline {PipelineId} not found", pipelineId);
				return null;
			}

			var exportObject = await _pipelinesDao.ExportPipeline(pipelineId);
			exportObject.PipelineName = infoDto.Name;
			return exportObject;
		}

		public async Task<Guid> ImportPipeline(PipelineExport exportObject)
		{
			Pipeline pipeline = null;
			var operationIds = new Dictionary<int, Guid>();

			foreach (var line in exportObject.PipelineData.Split('\n'))
			{
				// TODO: change this to Regex
				if (line.Contains("\"type\":\"node\""))
				{
					if (line.Contains($"\"{nameof(Pipeline)}\""))
					{
						_logger.LogDebug("Skipping pipeline node");
					}
					else if (pipeline == null)
					{
						_logger.LogWarning(
							"No pipeline object was found in the export file\nHint: Pipeline objects must occur before operations");
						throw new InvalidDataException("No pipeline object was found in the export file");
					}
					else if (line.Contains($"\"{nameof(Operation)}\""))
					{
						_logger.LogDebug("Skipping operation node");
					}
				}
				else if (line.Contains("\"type\":\"relationship\""))
				{
					var relationship = JsonConvert.DeserializeObject<Neo4JRelationShip<Pipeline, Operation>>(line);
					if (relationship == null)
					{
						throw new InvalidDataException("Expected relationship object");
					}

					if (pipeline == null)
					{
						pipeline = relationship.Start.Properties;
						pipeline.Id = Guid.NewGuid();
						pipeline.CreatedOn = DateTime.UtcNow;
						pipeline.Name = exportObject.PipelineName;
						pipeline.UserIdentifier = exportObject.CreatedBy;
						_logger.LogDebug("Found pipeline node {OriginalPipelineId} to {NewPipelineId}",
							relationship.Start.Properties.Id, pipeline.Id);
						await _pipelinesDao.CreatePipeline(pipeline);
					}

					var operation = relationship.End.Properties;
					operation.PipelineId = pipeline.Id;
					operation.Id = Guid.NewGuid();
					operationIds.Add(relationship.End.Id, operation.Id);
					await _pipelinesDao.CreateRootOperation(pipeline.Id, operation);
				}
				else
				{
					_logger.LogWarning("Found unknown	line in export file ({Line})", line);
				}
			}

			if (pipeline == null)
			{
				throw new InvalidDataException("No pipeline object found in provided data");
			}

			var lines = new Queue<string>(exportObject.OperationData.Split('\n'));

			while (lines.Count > 0)
			{
				var line = lines.Dequeue();
				// TODO: change this to Regex
				if (line.Contains("\"type\":\"node\""))
				{
					_logger.LogDebug("Skipping node");
				}
				else if (line.Contains("\"type\":\"relationship\""))
				{
					var relationship = JsonConvert.DeserializeObject<Neo4JRelationShip<Operation, Operation>>(line);
					var successor = relationship.End.Properties;
					if (operationIds.TryGetValue(relationship.End.Id, out var existingId))
					{
						_logger.LogDebug("Found existing operation {OperationId}", existingId);
						successor.Id = existingId;
					}
					else
					{
						successor.Id = Guid.NewGuid();
					}

					successor.PipelineId = pipeline.Id;
					if (operationIds.TryGetValue(relationship.Start.Id, out var rootOpId))
					{
						await _pipelinesDao.CreateSuccessor(new List<Guid> { rootOpId }, successor);
						if (!operationIds.TryAdd(relationship.End.Id, successor.Id))
						{
							_logger.LogDebug("Duplicate operation id found in export file");
						}
					}
					else
					{
						_logger.LogWarning("Could not find predecessors for operation {OperationId}", successor.Id);
						lines.Enqueue(line);
					}
				}
				else
				{
					_logger.LogWarning("Found unknown	line in export file ({Line})", line);
				}
			}

			return pipeline.Id;
		}

		public async Task<Guid> ImportPipelineCandidate(PipelineCandidate pipelineCandidate)
		{
			_logger.LogInformation("Importing pipeline candidate {PipelineCandidateId}", pipelineCandidate.PipelineId);

			if (pipelineCandidate.Aborted.HasValue && pipelineCandidate.Aborted.Value)
			{
				_logger.LogInformation("Pipeline candidate {PipelineCandidateId} is aborted", pipelineCandidate.PipelineId);
				return Guid.Empty;
			}

			var client = _httpClientFactory.CreateClient();
			client.BaseAddress = new Uri("https://old.openml.org");
			var openMlResponse = await client.GetAsync($"api/v1/json/task/{pipelineCandidate.TaskId}");
			var task = JsonConvert.DeserializeObject<OpenMlTaskResponse>(await openMlResponse.Content.ReadAsStringAsync());

			if (task?.Task?.Id == null)
			{
				_logger.LogInformation("Could not find task {TaskId}", pipelineCandidate.TaskId);
				return Guid.Empty;
			}

			_logger.LogDebug("Loaded task {TaskId}", task.Task.Id);

			var pipeline = new Pipeline
			{
				Id = pipelineCandidate.PipelineId,
				Name = $"[Simulated] {task.Task.Name}",
				CreatedOn = DateTime.UtcNow,
				UserIdentifier = pipelineCandidate.CreatedBy ?? "simulated"
			};

			await _pipelinesDao.CreatePipeline(pipeline);

			var operationDatasetMap = new Dictionary<string, PredecessorOperationDto>();
			foreach (var action in pipelineCandidate.Actions)
			{
				_logger.LogDebug(
					"Processing {@InputDatasets} on {OperationId} {OperationName} producing datasets {@OutputDatasets}",
					action.InputDatasets.Select(ds => new { ds.Type, ds.Key }),
					action.Operation.OperationId, action.Operation.OperationName,
					action.OutputDatasets.Select(ds => new { ds.Type, ds.Key }));

				var predecessorOperations = new List<PredecessorOperationDto>();
				foreach (var actionInputDataset in action.InputDatasets)
				{
					if (operationDatasetMap.TryGetValue(actionInputDataset.Key, out var operations))
					{
						predecessorOperations.Add(operations);
					}
					else
					{
						_logger.LogWarning("Input dataset {DatasetKey} not found as result of previous operation",
							actionInputDataset.Key);
					}
				}

				predecessorOperations = predecessorOperations
					.GroupBy(p => p.OperationId)
					.Select(g => new PredecessorOperationDto
					{
						OperationId = g.Key,
						OperationTemplateId = g.First().OperationTemplateId,
						OutputDatasets = g.Aggregate(new HashSet<Dataset>(), (acc, p) =>
							{
								acc.AddAll(p.OutputDatasets);
								return acc;
							})
							.ToList()
					}).ToList();

				var request = new AddOperationRequest
				{
					PipelineId = pipeline.Id,
					PredecessorOperationDtos = predecessorOperations,
					NewOperationTemplate = action.Operation,
					UserIdentifier = pipeline.UserIdentifier
				};

				var response = await _operationsService.AddOperationToPipeline(request);
				if (!response.Success)
				{
					_logger.LogWarning("Failed to add operation {OperationId} to pipeline {PipelineId} - {@Errors}",
						action.Operation.OperationId, pipeline.Id, response.Errors);
					continue;
				}

				if (response.ResultingDatasets.Count != action.OutputDatasets.Count)
				{
					throw new InvalidDataException(
						"Number of resulting datasets does not match number of expected output datasets - they should be the same");
				}

				for (var i = 0; i < action.OutputDatasets.Count; i++)
				{
					if (action.OutputDatasets[i].Type != response.ResultingDatasets[i].Type)
					{
						throw new InvalidDataException(
							"Type of resulting dataset does not match expected output dataset type - they should be the same");
					}

					_logger.LogDebug("Change id of action dataset {DatasetKey} to {NewDatasetKey}",
						action.OutputDatasets[i].Key, response.ResultingDatasets[i].Key);

					operationDatasetMap.Add(action.OutputDatasets[i].Key, new PredecessorOperationDto
					{
						OperationId = response.OperationId,
						OperationTemplateId = action.Operation.OperationId,
						OutputDatasets = new List<Dataset> { response.ResultingDatasets[i] }
					});
				}
			}

			return pipeline.Id;
		}
	}
}
