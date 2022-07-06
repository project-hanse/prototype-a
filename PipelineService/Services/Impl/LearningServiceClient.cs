using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PipelineService.Dao;
using PipelineService.Extensions;

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

		await AssertAllPipelinesExecuted();
		var request = new HttpRequestMessage(HttpMethod.Get, "api/train");
		Client.Timeout = TimeSpan.FromMinutes(15);
		var response = await Client.SendAsync(request);
		if (response.StatusCode != System.Net.HttpStatusCode.OK)
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

	private async Task AssertAllPipelinesExecuted()
	{
		_logger.LogDebug("Asserting all pipelines executed (meaning datasets are ready)");
		var pipelines = await _pipelinesDao.GetDtos();
		var progress = 0;
		foreach (var pipelineInfoDto in pipelines)
		{
			progress++;
			await _pipelineExecutionService.ExecutePipelineSync(pipelineInfoDto.Id, skipIfExecuted: true);
			_logger.LogDebug("Executed pipeline {Progress}/{Total}", progress, pipelines.Count);
		}

		_logger.LogInformation("All pipelines executed at least once");
	}
}
