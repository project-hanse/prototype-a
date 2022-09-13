using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Dao;
using PipelineService.Exceptions;
using PipelineService.Extensions;
using PipelineService.Models;
using PipelineService.Models.Dtos;
using PipelineService.Models.Enums;
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
		private readonly EfDatabaseContext _databaseContext;
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
			EfDatabaseContext databaseContext,
			IHttpClientFactory httpClientFactory)
		{
			_logger = logger;
			_configuration = configuration;
			_pipelinesDao = pipelinesDao;
			_pipelineExecutionService = pipelineExecutionService;
			_pipelineCandidateService = pipelineCandidateService;
			_operationsService = operationsService;
			_databaseContext = databaseContext;
			_httpClientFactory = httpClientFactory;
		}

		public async Task<IList<OperationTuples>> GetOperationTuples()
		{
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

			var pipelineInfoDto = await _pipelinesDao.GetInfoDto(pipelineCandidate.PipelineId);
			if (pipelineInfoDto != null)
			{
				_logger.LogInformation("Pipeline candidate {PipelineCandidateId} already exists", pipelineCandidate.PipelineId);
				return pipelineInfoDto.Id;
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

		public async Task ProcessIncompleteCandidatesInBackground()
		{
			_logger.LogDebug("Checking if incomplete candidates exist and enqueueing them for processing...");

			var incompleteMetricsIds = await _databaseContext.CandidateProcessingMetrics
				.Where(m => !m.ProcessingCompleted)
				.Select(m =>
					new
					{
						MetricId = m.Id,
						PipelineId = m.PipelineId
					})
				.ToListAsync();

			if (incompleteMetricsIds.Count == 0)
			{
				_logger.LogInformation("No incomplete pipeline candidates to process");
			}
			else
			{
				var averageProcessingTime = await _databaseContext.CandidateProcessingMetrics
					.Where(m => m.ProcessingCompleted && m.ProcessingDurationP != null)
					.AverageAsync(m => m.ProcessingDurationP);

				if (!averageProcessingTime.HasValue || averageProcessingTime < 5000)
				{
					averageProcessingTime = 5000;
					_logger.LogDebug("Average processing time is less than 5 seconds, setting it to 5 seconds");
				}

				_logger.LogDebug("Found {NumberOfIncompleteCandidates} incomplete pipeline candidates",
					incompleteMetricsIds.Count);
				var i = 0;
				foreach (var incompleteCandidate in incompleteMetricsIds)
				{
					i++;
					_logger.LogDebug("Enqueueing candidate {CandidateId} for processing in {ScheduledInSeconds}",
						incompleteCandidate.PipelineId, i * averageProcessingTime.Value / 1000);
					BackgroundJob.Schedule<IPipelinesDtoService>(s =>
							s.ProcessPipelineAsCandidate(incompleteCandidate.MetricId, incompleteCandidate.PipelineId),
						TimeSpan.FromMilliseconds(i * averageProcessingTime.Value));
				}

				_logger.LogInformation("Enqueued {NumberOfIncompleteCandidates} incomplete pipeline candidates for processing",
					incompleteMetricsIds.Count);
			}
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
					_logger.LogWarning(e, "Failed to process pipeline candidate {CandidateId} - {Error}",
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

		public async Task ProcessPipelineAsCandidate(Guid metricId, Guid pipelineId)
		{
			_logger.LogDebug("Processing pipeline {PipelineId} as candidate", pipelineId);
			var metric = await _databaseContext.CandidateProcessingMetrics.SingleOrDefaultAsync(m => m.Id == metricId);
			if (metric == null)
			{
				_logger.LogWarning("Candidate processing metric with id {MetricId} not found", metricId);
				return;
			}

			await RandomizeAndExecute(pipelineId, metric);
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
				BatchNumber = candidate.BatchNumber,
				SimulationStartTime = candidate.StartedAt,
				SimulationEndTime = candidate.CompletedAt,
				RewardFunctionType = candidate.RewardFunctionType,
				ExecutionAttempts = 1
			};

			_databaseContext.CandidateProcessingMetrics.Add(metric);

			_logger.LogInformation("Starting processing of pipeline candidate {PipelineCandidateId}...",
				candidate.PipelineId);
			if (candidate.Aborted.HasValue && candidate.Aborted.Value)
			{
				metric.Aborted = candidate.Aborted.Value;
				metric.Success = false;
				_logger.LogInformation("Pipeline candidate {PipelineCandidateId} was aborted - deleting candidate",
					candidate.PipelineId);
				await _pipelineCandidateService.DeletePipelineCandidate(candidate.PipelineId);
				metric.ProcessingEndTime = DateTime.UtcNow;
				_databaseContext.CandidateProcessingMetrics.Add(metric);
				await _databaseContext.SaveChangesAsync();
				return;
			}

			_logger.LogDebug("Creating pipeline from candidate {PipelineCandidateId}", candidate.PipelineId);

			metric.ImportStartTime = DateTime.UtcNow;
			var pipelineId = await ImportPipelineCandidate(candidate);
			metric.ImportSuccess = true;
			metric.ImportEndTime = DateTime.UtcNow;
			_logger.LogInformation("Imported pipeline candidate with id {PipelineCandidateId} in {CandidateImportDuration}",
				pipelineId, metric.ImportDuration);

			await _databaseContext.SaveChangesAsync();

			await RandomizeAndExecute(pipelineId, metric);
		}

		/// <summary>
		/// Starts a loop that tries to randomize the configurations of a pipelines operations and then executes the pipeline.
		/// This loop is aborted once the pipeline has been executed successfully or the maximum number of retries has been reached.
		/// </summary>
		/// <param name="pipelineId">The pipeline that will be randomized.</param>
		/// <param name="metric">An entity to store metrics about this process.</param>
		private async Task RandomizeAndExecute(Guid pipelineId, CandidateProcessingMetric metric)
		{
			try
			{
				_logger.LogInformation(
					"Starting randomization and execution loop for pipeline candidate {PipelineCandidateId}...",
					pipelineId);
				var pipelineInfo = await _pipelinesDao.GetInfoDto(pipelineId);
				if (pipelineInfo == null)
				{
					_logger.LogInformation("Pipeline candidate {PipelineCandidateId} does not exist - aborting processing",
						pipelineId);
					throw new NotFoundException("Pipeline does not exist");
				}

				metric.OperationCount = await _pipelinesDao.GetOperationCount(pipelineId);
				var executionRecord = await _pipelineExecutionService.ExecutePipelineSync(pipelineId);
				var maxVariationAttempts = _configuration.GetValue("PipelineCandidates:MaxVariantAttempts", 20);
				metric.OperationsRandomizedCount.Add(metric.ExecutionAttempts, 0);
				var previousOperationConfig = new Dictionary<Guid, IDictionary<string, string>>();
				var previousSuccessfullyOperations = executionRecord.OperationExecutionRecords.Count(o => o.IsSuccessful);
				while (!executionRecord.IsSuccessful && metric.ExecutionAttempts < maxVariationAttempts)
				{
					metric.ExecutionAttempts++;
					_logger.LogInformation(
						"Initial execution failed, trying variants of the pipeline candidate {PipelineCandidateId} ({VariantAttempts}/{MaxVariantAttempts})...",
						pipelineId, metric.ExecutionAttempts, maxVariationAttempts);

					IList<Guid> operationsToRandomize;
					if (metric.ExecutionAttempts % 2 == 0)
					{
						// simple strategy 1: randomize configuration of all failed operations
						_logger.LogDebug("Randomizing configuration of all failed operations");
						operationsToRandomize = executionRecord.OperationExecutionRecords
							.Where(o => o.Status == ExecutionStatus.Failed)
							.Select(o => o.OperationId).ToList();
					}
					else
					{
						// simple strategy 2: randomize successfully executed operations except root operations
						_logger.LogDebug(
							"Randomizing configuration of all successfully executed operations except root operations");
						operationsToRandomize = executionRecord.OperationExecutionRecords
							.Where(o => o.Status == ExecutionStatus.Failed)
							.Select(o => o.OperationId).ToList();
					}

					foreach (var operationId in operationsToRandomize)
					{
						_logger.LogDebug("Randomizing configuration of operation {OperationId}", operationId);
						previousOperationConfig[operationId] = await _operationsService.GetConfig(pipelineId, operationId);
						var config = await _operationsService.GenerateRandomizedConfig(operationId);
						await _operationsService.UpdateConfig(pipelineId, operationId, config);
					}

					metric.OperationsRandomizedCount.Add(metric.ExecutionAttempts, operationsToRandomize.Count);

					_logger.LogInformation(
						"Saving metrics before executing pipeline {PipelineId} {TimesExecuted}/{TimesToExecute} times...",
						metric.PipelineId, metric.ExecutionAttempts, maxVariationAttempts);
					await _databaseContext.SaveChangesAsync();

					executionRecord = await _pipelineExecutionService.ExecutePipelineSync(pipelineId);
					if (executionRecord.OperationExecutionRecords.Count(o => o.IsSuccessful) < previousSuccessfullyOperations)
					{
						_logger.LogInformation(
							"Previous operation configuration variant produced more successful operations {PreviousSuccessfulOperations} > {SuccessfulOperations}, reverting changes on {OperationsToBeRevertedCount} operations...",
							previousSuccessfullyOperations, executionRecord.OperationExecutionRecords.Count(o => o.IsSuccessful),
							previousOperationConfig.Count);
						foreach (var (operationId, configuration) in previousOperationConfig)
						{
							await _operationsService.UpdateConfig(pipelineId, operationId, configuration);
						}
					}
					else
					{
						_logger.LogInformation(
							"New operation configuration variant produced at least as many successful operations as previous variation {PreviousSuccessfulOperations} <= {SuccessfulOperations}, keeping them...",
							previousSuccessfullyOperations, executionRecord.OperationExecutionRecords.Count(o => o.IsSuccessful));
						previousSuccessfullyOperations = executionRecord.OperationExecutionRecords.Count(o => o.IsSuccessful);
					}

					previousOperationConfig.Clear();
				}

				metric.Success = executionRecord.IsSuccessful;
				if (metric.Success)
				{
					metric.ProcessingEndTime = executionRecord.OperationExecutionRecords
						.Where(o => o.Status == ExecutionStatus.Succeeded)
						.Max(o => o.ExecutionCompletedAt);
				}
				else
				{
					var failedOperationRecord = executionRecord.OperationExecutionRecords
						.OrderBy(o => o.ExecutionCompletedAt)
						.FirstOrDefault(o => o.Status == ExecutionStatus.Failed);
					metric.ProcessingEndTime = executionRecord.OperationExecutionRecords
						.Where(o => o.Status == ExecutionStatus.Failed)
						.Max(o => o.ExecutionCompletedAt);
					metric.ErrorMessage = failedOperationRecord != default
						? $"Operation {failedOperationRecord.OperationId} (name: {failedOperationRecord.OperationIdentifier}) failed"
						: $"{executionRecord.OperationExecutionRecords.Count(o => o.Status == ExecutionStatus.Failed)} operations failed";
				}

				await _pipelineCandidateService.ArchivePipelineCandidate(pipelineId);
			}
			catch (Exception e)
			{
				metric.Success = false;
				metric.ErrorMessage = $"{e.GetType().Name} - {e.Message}";
				_logger.LogInformation("Failed to process pipeline candidate with id {PipelineCandidateId} - {Error}",
					pipelineId, e.Message);
			}
			finally
			{
				metric.ProcessingEndTime = DateTime.UtcNow;
				metric.ProcessingCompleted = true;
				metric.ProcessingDurationP = metric.ProcessingDuration;

				_logger.LogInformation("Finished processing pipeline candidate with id {PipelineCandidateId} in {Duration}",
					pipelineId, metric.ProcessingDuration);

				if (!metric.Success)
				{
					_logger.LogInformation(
						"Pipeline {PipelineCandidateId} could not be processed (Error: {Error}) - deleting pipeline",
						pipelineId, metric.ErrorMessage);
					await _pipelineExecutionService.DeletePipeline(pipelineId);
				}

				_databaseContext.CandidateProcessingMetrics.Update(metric);
				await _databaseContext.SaveChangesAsync();
			}
		}
	}
}
