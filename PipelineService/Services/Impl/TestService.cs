using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    public class TestService : ITestService
    {
        private readonly ILogger<TestService> _logger;

        public TestService(ILogger<TestService> logger)
        {
            _logger = logger;
        }

        public Task NewMessage(BlockExecutionResponse message)
        {
            _logger.LogInformation("New message: {message}", message);

            return Task.CompletedTask;
        }
    }
}