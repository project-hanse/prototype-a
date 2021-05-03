using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    public class HostedSubscriptionService : IHostedSubscriptionService
    {
        private readonly ILogger<HostedSubscriptionService> _logger;
        private readonly EventBusService _eventBusService;
        private readonly IPipelineExecutionService _pipelineExecutionService;

        public HostedSubscriptionService(
            ILogger<HostedSubscriptionService> logger,
            EventBusService eventBusService,
            IPipelineExecutionService pipelineExecutionService)
        {
            _logger = logger;
            _eventBusService = eventBusService;
            _pipelineExecutionService = pipelineExecutionService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting up subscriptions on MQTT topics...");

            cancellationToken.ThrowIfCancellationRequested();

            await _eventBusService.Subscribe<NodeExecutionResponse>(
                "executed/+/+",
                async m => { await _pipelineExecutionService.HandleExecutionResponse(m); });
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down...");
            cancellationToken.ThrowIfCancellationRequested();

            // TODO: shutdown client
            await _eventBusService.StopAsync();
        }
    }
}