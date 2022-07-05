using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Extensions;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Metrics;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl
{
	public class PipelinesDtoService : IPipelinesDtoService
	{
		private readonly ILogger<PipelinesDtoService> _logger;
		private readonly IConfiguration _configuration;
		private readonly IPipelinesDao _pipelinesDao;
		private readonly IPipelineExecutionService _pipelineExecutionService;
		private readonly IPipelineCandidateService _pipelineCandidateService;
		private readonly IOperationsService _operationsService;
		private readonly EfMetricsContext _metricsContext;
		private readonly IHttpClientFactory _httpClientFactory;

		public string DefaultPipelinesPath => Path.Combine(
			_configuration.GetValue(WebHostDefaults.ContentRootKey, ""),
			_configuration.GetValue("DefaultPipelinesFolder", ""));

		public PipelinesDtoService(ILogger<PipelinesDtoService> logger,
			IConfiguration configuration,
			IPipelinesDao pipelinesDao,
			IPipelineExecutionService pipelineExecutionService,
			IPipelineCandidateService pipelineCandidateService,
			IOperationsService operationsService,
			EfMetricsContext metricsContext,
			IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_configuration = configuration;
			_pipelinesDao = pipelinesDao;
			_pipelineExecutionService = pipelineExecutionService;
			_pipelineCandidateService = pipelineCandidateService;
			_operationsService = operationsService;
			_metricsContext = metricsContext;
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

		public async Task<Guid> ImportPipelineCandidate(PipelineCandidate pipelineCandidate, string username = null)
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
				UserIdentifier = username ?? pipelineCandidate.CreatedBy ?? "simulated"
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

				var updateConfig = await _operationsService
					.UpdateConfig(pipeline.Id, response.OperationId, action.Operation.DefaultConfig);
				if (!updateConfig)
				{
					_logger.LogWarning("Failed to update configuration for operation {OperationId} in pipeline {PipelineId}",
						action.Operation.OperationId, pipeline.Id);
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

		public async Task<int> ProcessPipelineCandidates(int numberOfCandidates)
		{
			var candidates = await _pipelineCandidateService.GetPipelineCandidates(new Pagination()
			{
				Page = 1,
				PageSize = numberOfCandidates
			});

			return await ProcessPipelineCandidates(candidates);
		}

		public async Task<int> ProcessPipelineCandidates(IList<Guid> numberOfCandidates)
		{
			var candidates = new List<PipelineCandidate>();
			foreach (var candidateId in numberOfCandidates)
			{
				candidates.Add(await _pipelineCandidateService.GetCandidateById(candidateId));
			}

			return await ProcessPipelineCandidates(candidates);
		}

		private async Task<int> ProcessPipelineCandidates(ICollection<PipelineCandidate> candidates)
		{
			_logger.LogDebug("Processing {NumberOfCandidates} pipeline candidates", candidates.Count);
			var processed = 0;
			foreach (var candidate in candidates)
			{
				try
				{
					await ProcessPipelineCandidate(candidate);
					processed++;
				}
				catch (Exception e)
				{
					_logger.LogWarning(e,"Failed to process pipeline candidate {CandidateId} - {Error}",
						candidate?.PipelineId ?? default, e.Message);
					if (candidate != null)
					{
						await _pipelineCandidateService.DeletePipelineCandidate(candidate.PipelineId);
					}
				}
			}

			_logger.LogInformation("Processed {Processed} pipeline candidates", processed);
			BackgroundJob.Enqueue<ILearningServiceClient>(s => s.TriggerModelTraining());
			return processed;
		}

		private async Task ProcessPipelineCandidate(PipelineCandidate candidate)
		{
			_logger.LogDebug("Processing pipeline candidate with id {PipelineCandidateId}", candidate.PipelineId);
			var metric = new CandidateProcessingMetric
			{
				CandidateCreatedOn = candidate.CompletedAt,
				ProcessingStartTime = DateTime.UtcNow,
				TaskId = candidate.TaskId,
				PipelineId = candidate.PipelineId,
				ActionCount = candidate.Actions.Count,
				BatchNumber = candidate.BatchNumber
			};
			try
			{
				metric.ImportStartTime = DateTime.UtcNow;
				var pipelineId = await ImportPipelineCandidate(candidate);
				metric.ImportSuccess = true;
				metric.ImportEndTime = DateTime.UtcNow;
				_logger.LogInformation("Imported pipeline candidate with id {PipelineCandidateId} in {CandidateImportDuration}",
					candidate.PipelineId, metric.ImportDuration);

				var executionRecord = await _pipelineExecutionService.ExecutePipelineSync(pipelineId);
				metric.Success = executionRecord.Failed.Count == 0;
				if (metric.Success)
				{
					var lastOperation = executionRecord.Executed.LastOrDefault();
					metric.ProcessingEndTime = lastOperation?.ExecutionCompletedAt ?? DateTime.UtcNow;
				}
				else
				{
					var failedOperationRecord = executionRecord.Failed.FirstOrDefault();
					metric.ProcessingEndTime = failedOperationRecord?.ExecutionCompletedAt ?? DateTime.UtcNow;
					metric.Error = failedOperationRecord != default
						? $"Operation {failedOperationRecord.OperationId} (name: {failedOperationRecord.Name}) failed"
						: $"{executionRecord.Failed.Count} operations failed";
				}

				await _pipelineCandidateService.DeletePipelineCandidate(candidate.PipelineId);
			}
			catch (Exception e)
			{
				metric.Success = false;
				metric.Error = e.Message;
				_logger.LogInformation("Failed to process pipeline candidate with id {PipelineCandidateId} - {Error}",
					candidate.PipelineId, e.Message);

				await _pipelinesDao.DeletePipeline(candidate.PipelineId);
			}
			finally
			{
				metric.ProcessingEndTime = DateTime.UtcNow;

				_logger.LogInformation("Finished processing pipeline candidate with id {PipelineCandidateId} in {Duration}",
					candidate.PipelineId, metric.ProcessingDuration);

				_metricsContext.CandidateProcessingMetrics.Add(metric);
				await _metricsContext.SaveChangesAsync();
			}
		}
	}
}
