using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client.Options;
using MQTTnet.Extensions.ManagedClient;

namespace PipelineService.Services.Impl
{
	public abstract class BaseEventBusService
	{
		private readonly ILogger<BaseEventBusService> _logger;
		protected IManagedMqttClient Client;
		protected abstract string Hostname { get; }
		protected abstract int Port { get; }
		protected abstract string ClientId { get; }
		protected abstract string Username { get; }
		protected abstract string Password { get; }

		protected BaseEventBusService(ILogger<BaseEventBusService> logger)
		{
			_logger = logger;
		}

		protected async Task ConnectAsync()
		{
			if (Client != null)
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

			Client = new MqttFactory().CreateManagedMqttClient();

			Client.UseConnectedHandler(e =>
			{
				_logger.LogDebug("Connected to MQTT broker ({Hostname}:{Port}) {@EventArgs}",
					Hostname, Port, e);
			});

			Client.UseDisconnectedHandler(e =>
			{
				_logger.LogDebug("Disconnected from MQTT broker ({Hostname}:{Port}) as client {ClientId} {@EventArgs}",
					Hostname, Port, ClientId, e);
			});

			Client.UseApplicationMessageReceivedHandler(e =>
			{
				_logger.LogDebug("Received payload from ({Hostname}:{Port}) on topic {@EventArgs}",
					Hostname, Port, e);
			});

			await Client.StartAsync(managedMqttClientOptions);
		}
	}
}
