using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
	public class HostedSubscriptionService : IHostedSubscriptionService
	{
		private string OperationExecutedTopic =>
			_configuration.GetValue("QueueNames:OperationExecuted", "operation/executed");

		private readonly ILogger<HostedSubscriptionService> _logger;
		private readonly IConfiguration _configuration;
		private readonly EventBusService _eventBusService;
		private readonly IPipelineExecutionService _pipelineExecutionService;

		public HostedSubscriptionService(
			ILogger<HostedSubscriptionService> logger,
			IConfiguration configuration,
			EventBusService eventBusService,
			IPipelineExecutionService pipelineExecutionService)
		{
			_logger = logger;
			_configuration = configuration;
			_eventBusService = eventBusService;
			_pipelineExecutionService = pipelineExecutionService;
		}

		public async Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Setting up subscriptions on MQTT topics...");

			cancellationToken.ThrowIfCancellationRequested();

			await _eventBusService.Subscribe<OperationExecutedMessage>(
				OperationExecutedTopic,
				async m => { await _pipelineExecutionService.HandleExecutionResponse(m); });
		}

		public async Task StopAsync(CancellationToken cancellationToken)
		{
			_logger.LogInformation("Shutting down...");
			cancellationToken.ThrowIfCancellationRequested();

			await _eventBusService.StopAsync();
		}
	}
}
