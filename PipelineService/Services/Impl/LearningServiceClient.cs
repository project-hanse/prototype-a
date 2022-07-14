using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Extensions;
using PipelineService.Models.Pipeline.Execution;

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

		await EnqueueAllPipelinesForExecution();
		BackgroundJob.Schedule<ILearningServiceClient>(s => s.TrainModels(), TimeSpan.FromMinutes(15));
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

	private async Task EnqueueAllPipelinesForExecution()
	{
		_logger.LogDebug("Asserting all pipelines executed (meaning datasets are ready)");
		var pipelines = await _pipelinesDao.GetDtos();
		var progress = 0;
		foreach (var pipelineInfoDto in pipelines)
		{
			progress++;
			await _pipelineExecutionService.ExecutePipeline(pipelineInfoDto.Id, true);
			_logger.LogDebug("Enqueued pipeline {Progress}/{Total}", progress, pipelines.Count);
		}
	}
}
