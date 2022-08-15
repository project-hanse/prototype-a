using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Extensions;
using PipelineService.Models.Enums;

namespace PipelineService.Services.Impl;

public class LearningServiceClient : ILearningServiceClient
{
	private readonly ILogger<LearningServiceClient> _logger;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _clientFactory;
	private readonly IPipelineExecutionService _pipelineExecutionService;
	private readonly IPipelinesDao _pipelinesDao;

	private HttpClient _client;

	private HttpClient Client
	{
		get
		{
			if (_client != null) return _client;
			_client = _clientFactory.CreateClient("learning-service");
			_client.BaseAddress = new Uri(_configuration.GetValueOrThrow<string>("LearningService:RootUri"));
			return _client;
		}
	}

	public LearningServiceClient(
		ILogger<LearningServiceClient> logger,
		IConfiguration configuration,
		IHttpClientFactory clientFactory,
		IPipelineExecutionService pipelineExecutionService,
		IPipelinesDao pipelinesDao)
	{
		_logger = logger;
		_configuration = configuration;
		_clientFactory = clientFactory;
		_pipelineExecutionService = pipelineExecutionService;
		_pipelinesDao = pipelinesDao;
	}

	public async Task TriggerModelTraining()
	{
		_logger.LogDebug("Triggering model training in learning service");
		var parallel = _configuration.GetValue("LearningService:PipelinesInParallel", 4);
		var totalDelay = await EnqueueAllPipelinesForExecution(parallel);
		_logger.LogInformation("Schedule model training in {TotalDelay} minutes at {TrainingTime}",
			totalDelay, DateTime.Now.AddMinutes(totalDelay));
		BackgroundJob.Schedule<ILearningServiceClient>(s => s.TrainModels(), TimeSpan.FromMinutes(totalDelay + 1));
	}

	public async Task TrainModels()
	{
		var request = new HttpRequestMessage(HttpMethod.Get, "api/train");
		Client.Timeout = TimeSpan.FromMinutes(15);
		var response = await Client.SendAsync(request);
		if (response.StatusCode != HttpStatusCode.OK)
		{
			_logger.LogWarning(
				"Error triggering model training in learning service (status: {HttpStatusCode}, reason: {ReasonPhrase})",
				response.StatusCode, response.ReasonPhrase);
		}
		else
		{
			_logger.LogInformation("Model training triggered in learning service");
		}
	}

	private async Task<int> EnqueueAllPipelinesForExecution(int parallel)
	{
		_logger.LogDebug("Asserting all pipelines executed (meaning datasets are ready)");
		var pipelines = (await _pipelinesDao.GetDtos()).Items;
		var progress = 0;
		var delay = 0;
		foreach (var pipelineInfoDto in pipelines)
		{
			progress++;
			if (await _pipelineExecutionService.HasBeenExecuted(pipelineInfoDto.Id))
			{
				_logger.LogDebug("Pipeline {PipelineId} has been executed", pipelineInfoDto.Id);
				continue;
			}

			BackgroundJob.Schedule<IPipelineExecutionService>(
				s => s.ExecutePipeline(pipelineInfoDto.Id, true, ExecutionStrategy.Lazy),
				TimeSpan.FromMinutes(delay));
			_logger.LogInformation("Enqueued pipeline {PipelineId} in {Delay} minutes ({Progress}/{Total})",
				pipelineInfoDto.Id, delay, progress, pipelines.Count);

			if (progress % parallel == 0) delay++;
		}

		return delay;
	}
}
