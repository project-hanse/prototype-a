using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    public class HostedSubscriptionService : IHostedSubscriptionService
    {
        private readonly ILogger<HostedSubscriptionService> _logger;
        private readonly IMqttMessageService _messageService;
        private readonly IPipelineExecutionService _pipelineExecutionService;

        public HostedSubscriptionService(
            ILogger<HostedSubscriptionService> logger,
            IMqttMessageService messageService,
            IPipelineExecutionService pipelineExecutionService)
        {
            _logger = logger;
            _messageService = messageService;
            _pipelineExecutionService = pipelineExecutionService;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Setting up subscriptions on MQTT topics...");

            cancellationToken.ThrowIfCancellationRequested();

            await _messageService.Subscribe<SimpleBlockExecutionResponse>(
                "executed/+/+",
                async m => { await _pipelineExecutionService.HandleExecutionResponse(m); });
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Shutting down...");
            
            // TODO: shutdown client

            return Task.CompletedTask;
        }
    }
}