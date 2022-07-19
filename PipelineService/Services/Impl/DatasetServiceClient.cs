using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PipelineService.Extensions;
using PipelineService.Models.Dtos;
using PipelineService.Models.Pipeline;

namespace PipelineService.Services.Impl;

public class DatasetServiceClient : IDatasetServiceClient
{
	private readonly ILogger<DatasetServiceClient> _logger;
	private readonly IConfiguration _configuration;
	private readonly IHttpClientFactory _clientFactory;

	private HttpClient _client;

	private HttpClient Client
	{
		get
		{
			if (_client != null) return _client;
			_client = _clientFactory.CreateClient("dataset-service");
			_client.BaseAddress = new Uri(_configuration.GetValueOrThrow<string>("DatasetService:RootUri"));
			return _client;
		}
	}

	public DatasetServiceClient(
		ILogger<DatasetServiceClient> logger,
		IConfiguration configuration,
		IHttpClientFactory clientFactory)
	{
		_logger = logger;
		_configuration = configuration;
		_clientFactory = clientFactory;
	}

	public async Task<DatasetMetadataCompact> GetCompactMetadata(Dataset dataset)
	{
		_logger.LogDebug("Getting compact metadata for dataset {DatasetKey}", dataset.Key);
		var request =
			new HttpRequestMessage(HttpMethod.Get, "api/metadata/key/" + dataset.Key + "?format=json&version=compact");
		var response = await Client.SendAsync(request);
		if (!response.IsSuccessStatusCode)
		{
			_logger.LogInformation("Failed to get compact metadata for dataset {DatasetKey} - status: {StatusCode}",
				dataset.Key, response.StatusCode);
			return null;
		}

		var json = await response.Content.ReadAsStringAsync();
		return JsonConvert.DeserializeObject<DatasetMetadataCompact>(json);
	}
}
