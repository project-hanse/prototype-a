using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using Newtonsoft.Json;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    /// <summary>
    /// A service that implements communication with the internal event bus (message broker).
    /// </summary>
    public class EventBusService : IEventBusService
    {
        private readonly ILogger<EventBusService> _logger;
        private readonly IConfiguration _configuration;
        private IManagedMqttClient _client;

        private string Hostname => _configuration.GetValue("EVENT_BUS:MQTT_HOST", "message-broker");
        private int Port => _configuration.GetValue("EVENT_BUS:MQTT_PORT", 1883);

        private string ClientId =>
            _configuration.GetValue("EVENT_BUS:MQTT_CLIENT_ID", $"PipelineService-{Guid.NewGuid()}");

        private string Username => _configuration.GetValue<string>("EVENT_BUS:MQTT_USER", null);
        private string Password => _configuration.GetValue<string>("EVENT_BUS:MQTT_PASSWORD", null);

        public EventBusService(
            ILogger<EventBusService> logger,
            IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        private async Task ConnectAsync()
        {
            if (_client != null)
            {
                _logger.LogDebug("MQTT Client already exists");
                return;
            }

            _logger.LogInformation(
                "Setting up client for MQTT broker ({Hostname}:{Port}) with client id {ClientId}...",
                Hostname, Port, ClientId);

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(ClientId)
                .WithTcpServer(Hostname, Port);

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                _logger.LogDebug("Using authentication with username {Username}", Username);

                mqttClientOptionsBuilder.WithCredentials(Username, Password);
            }

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(mqttClientOptionsBuilder.Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();

            _client.UseConnectedHandler(e =>
            {
                _logger.LogDebug("Disconnected from MQTT broker ({Hostname}:{Port}) {@EventArgs}",
                    Hostname, Port, e);
            });

            _client.UseDisconnectedHandler(e =>
            {
                _logger.LogDebug("Connected to MQTT broker ({Hostname}:{Port}) as client {ClientId} {@EventArgs}",
                    Hostname, Port, ClientId, e);
            });

            _client.UseApplicationMessageReceivedHandler(e =>
            {
                _logger.LogDebug("Received payload from ({Hostname}:{Port}) on topic {@EventArgs}",
                    Hostname, Port, e);
            });

            await _client.StartAsync(managedMqttClientOptions);
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

            await _client.PublishAsync(mqttMessage);
        }

        public async Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage
        {
            await ConnectAsync();

            _logger.LogInformation("Subscribing to MQTT topic {Topic}", topic);

            var topicFilter = new MqttTopicFilterBuilder()
                .WithTopic(topic)
                .WithQualityOfServiceLevel(0)
                .Build();

            await _client.SubscribeAsync(topicFilter);

            _client.UseApplicationMessageReceivedHandler(async a =>
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