using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
	public class EdgeEventBusService : BaseEventBusService, IEventBusService
	{
		private readonly ILogger<EdgeEventBusService> _logger;
		private readonly IConfiguration _configuration;

		protected override string Hostname =>
			_configuration.GetValue("EDGE_EVENT_BUS:MQTT_HOST", "edge-message-broker");

		protected override int Port => _configuration.GetValue("EDGE_EVENT_BUS:MQTT_PORT", 1884);

		protected override string ClientId =>
			_configuration.GetValue("EDGE_EVENT_BUS:MQTT_CLIENT_ID", $"PipelineService-{Guid.NewGuid()}");

		protected override string Username => _configuration.GetValue<string>("EDGE_EVENT_BUS:MQTT_USER", null);
		protected override string Password => _configuration.GetValue<string>("EDGE_EVENT_BUS:MQTT_PASSWORD", null);

		public EdgeEventBusService(ILogger<EdgeEventBusService> logger, IConfiguration configuration) : base(logger)
		{
			_logger = logger;
			_configuration = configuration;
		}

		public async Task PublishMessage<T>(string topic, T payload) where T : BaseMqttMessage
		{
			await ConnectAsync();

			_logger.LogInformation("Publishing message to topic {Topic}", topic);

			// TODO implement quality of service 2 (Exactly once) - requires acknowledgement from receiver
			var mqttMessage = new MqttApplicationMessageBuilder()
				.WithTopic(topic)
				.WithPayload(JsonConvert.SerializeObject(payload))
				.WithQualityOfServiceLevel(0)
				.WithRetainFlag()
				.Build();

			await Client.PublishAsync(mqttMessage);
		}

		public async Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage
		{
			await ConnectAsync();

			_logger.LogInformation("Subscribing to MQTT topic {Topic} from the edge", topic);

			var topicFilter = new MqttTopicFilterBuilder()
				.WithTopic(topic)
				.WithQualityOfServiceLevel(0)
				.Build();

			await Client.SubscribeAsync(topicFilter);

			Client.UseApplicationMessageReceivedHandler(async a =>
			{
				try
				{
					var message =
						JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(a.ApplicationMessage.Payload));

					await handler(message);
				}
				catch (Exception e)
				{
					// TODO: Do proper exception handling
					Console.Error.WriteLine(e);
				}
			});
		}
	}
}
