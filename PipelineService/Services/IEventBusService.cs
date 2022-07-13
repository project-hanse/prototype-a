using System;
using System.Threading.Tasks;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services
{
	/// <summary>
	/// A service that sends and receives messages (e.g. MQTT, AMQP) from and to the appropriate message broker.
	/// </summary>
	public interface IEventBusService
	{
		public Task PublishMessage<T>(string topic, T payload) where T : BaseMqttMessage;

		public Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage;
	}
}
