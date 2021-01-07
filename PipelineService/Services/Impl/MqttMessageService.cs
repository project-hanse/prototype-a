using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;
using PipelineService.Models.MqttMessages;

namespace PipelineService.Services.Impl
{
    public class MqttMessageService : IMqttMessageService
    {
        private readonly ILogger<MqttClient> _logger;
        private readonly IConfiguration _configuration;
        private IManagedMqttClient _client;

        private string Hostname => _configuration.GetValue("MQTT_HOST", "message-broker");
        private int Port => _configuration.GetValue("MQTT_PORT", 1883);
        private string ClientId => _configuration.GetValue("MQTT_CLIENT_ID", $"PipelineService-{Guid.NewGuid()}");
        private string Username => _configuration.GetValue<string>("MQTT_USER", null);
        private string Password => _configuration.GetValue<string>("MQTT_PASSWORD", null);

        public MqttMessageService(
            ILogger<MqttClient> logger,
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
                $"Setting up client for MQTT broker ({Hostname}:{Port}) with client id {ClientId}...");

            var mqttClientOptionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(ClientId)
                .WithTcpServer(Hostname, Port);

            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                _logger.LogDebug($"Using authentication with username {Username}");

                mqttClientOptionsBuilder.WithCredentials(Username, Password);
            }

            var managedMqttClientOptions = new ManagedMqttClientOptionsBuilder()
                .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
                .WithClientOptions(mqttClientOptionsBuilder.Build())
                .Build();

            _client = new MqttFactory().CreateManagedMqttClient();

            _client.UseConnectedHandler(e =>
            {
                _logger.LogDebug($"Disconnected from MQTT broker ({Hostname}:{Port})", e);
            });

            _client.UseDisconnectedHandler(e =>
            {
                _logger.LogDebug($"Connected to MQTT broker ({Hostname}:{Port}) as client {ClientId}", e);
            });

            _client.UseApplicationMessageReceivedHandler(e =>
            {
                _logger.LogDebug(
                    $"Received payload from ({Hostname}:{Port}) on topic {e.ApplicationMessage.Topic}");
            });

            await _client.StartAsync(managedMqttClientOptions);
        }

        public async Task PublishMessage<T>(string topic, T payload) where T : MqttBaseMessage
        {
            await ConnectAsync();

            _logger.LogInformation($"Publishing message to topic {topic}");

            var mqttMessage = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(JsonSerializer.Serialize(payload))
                .WithExactlyOnceQoS()
                .WithRetainFlag()
                .Build();

            await _client.PublishAsync(mqttMessage);
        }

        public Task Subscribe(string topic)
        {
            throw new NotImplementedException();
        }
    }
}