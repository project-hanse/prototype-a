using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    /// <summary>
    /// A service that implements communication with the internal event bus (message broker).
    /// </summary>
    public class EventBusService : BaseEventBusService, IEventBusService
    {
        private readonly ILogger<EventBusService> _logger;
        private readonly IConfiguration _configuration;

        protected override string Hostname => _configuration.GetValue("EVENT_BUS:MQTT_HOST", "message-broker");
        protected override int Port => _configuration.GetValue("EVENT_BUS:MQTT_PORT", 1883);

        protected override string ClientId =>
            _configuration.GetValue("EVENT_BUS:MQTT_CLIENT_ID", $"PipelineService-{Guid.NewGuid()}");

        protected override string Username => _configuration.GetValue<string>("EVENT_BUS:MQTT_USER", null);
        protected override string Password => _configuration.GetValue<string>("EVENT_BUS:MQTT_PASSWORD", null);

        public EventBusService(
            ILogger<EventBusService> logger,
            IConfiguration configuration) : base(logger)
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
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await Client.PublishAsync(mqttMessage);
        }

        public async Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage
        {
            await ConnectAsync();

            _logger.LogInformation("Subscribing to MQTT topic {Topic}", topic);

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.ExactlyOnce)
                .Build();

            await Client.SubscribeAsync(topicFilter);

            Client.UseApplicationMessageReceivedHandler(async a =>
            {
                try
                {
                    var message =
                        JsonConvert.DeserializeObject<T>(Encoding.Default.GetString(a.ApplicationMessage.Payload),
                            new JsonSerializerSettings
                            {
                                TypeNameHandling = TypeNameHandling.Auto,
                                NullValueHandling = NullValueHandling.Ignore,
                            });

                    await handler(message);
                }
                catch (JsonSerializationException e)
                {
                    // TODO: Do proper exception handling
                    _logger.LogError("Failed to deserialize message - {@ErrorMessage}", e);
                }
                catch (Exception e)
                {
                    _logger.LogError("General error while handling message - {@ErrorMessage}", e);
                }
            });
        }

        public async Task StopAsync()
        {
            _logger.LogInformation("Stopping MQTT client");
            await Client.StopAsync();
        }
    }
}