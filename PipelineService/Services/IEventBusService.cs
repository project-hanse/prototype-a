using System;
using System.Threading.Tasks;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services
{
    /// <summary>
    /// A wrapper service around a managed MQTT client that handles setting up the client.
    /// </summary>
    public interface IEventBusService
    {
        public Task PublishMessage<T>(string topic, T payload) where T : BaseMqttMessage;

        public Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage;
    }
}