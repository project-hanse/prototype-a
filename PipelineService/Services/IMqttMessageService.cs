using System.Threading.Tasks;
using PipelineService.Models;

namespace PipelineService.Services
{
    /// <summary>
    /// A wrapper service around a managed MQTT client that handles setting up the client.
    /// </summary>
    public interface IMqttMessageService
    {
        public Task PublishMessage(string topic, MqttMessage payload);
    }
}