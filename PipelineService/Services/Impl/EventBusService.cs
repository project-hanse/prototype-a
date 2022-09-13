using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PipelineService.Services.Impl
{
	/// <summary>
	/// A service that implements communication with the internal event bus (message broker).
	/// </summary>
	public class EventBusService : BaseRabbitMqEventBusService
	{
		private readonly ILogger<EventBusService> _logger;
		private readonly IConfiguration _configuration;

		protected override string Hostname => _configuration.GetValue("EVENT_BUS:HOST", "rabbitmq");
		protected override int Port => _configuration.GetValue("EVENT_BUS:PORT", 5672);

		private static readonly Guid ClientGuid = Guid.NewGuid();

		protected override string ClientId =>
			_configuration.GetValue("EVENT_BUS:CLIENT_ID", $"pipeline-service-{ClientGuid}");

		protected override string Username => _configuration.GetValue<string>("EVENT_BUS:USER", "guest");
		protected override string Password => _configuration.GetValue<string>("EVENT_BUS:PASSWORD", "guest");

		public EventBusService(
			ILogger<EventBusService> logger,
			IConfiguration configuration) : base(logger)
		{
			_logger = logger;
			_configuration = configuration;
		}

		public override async Task PublishMessage<T>(string topic, T payload)
		{
			_logger.LogDebug("Publishing message to topic {Topic}", topic);

			var channel = await GetChannel(topic);
			var properties = channel.CreateBasicProperties();
			properties.Persistent = true;
			var ttl = _configuration.GetValue<int?>("EVENT_BUS:MESSAGE_TTL", 60 * 60 * 2); // default to 2 hours
			if (ttl.HasValue)
			{
				properties.Expiration = (ttl.Value * 1000).ToString();
			}

			channel.BasicPublish(exchange: string.Empty,
				routingKey: topic,
				mandatory: true,
				basicProperties: properties,
				body: Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload)));
		}

		public override async Task Subscribe<T>(string topic, Func<T, Task> handler)
		{
			_logger.LogDebug("Subscribing to topic {Topic}", topic);
			var channel = await GetChannel(topic);
			var consumer = new AsyncEventingBasicConsumer(channel);

			var tag = channel.BasicConsume(topic, true, consumer);

			consumer.Received += async (model, ea) =>
			{
				_logger.LogDebug("Received message on topic {Topic}", topic);
				try
				{
					var body = ea.Body.ToArray();
					var message =
						JsonConvert.DeserializeObject<T>(Encoding.UTF8.GetString(body),
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
			};

			_logger.LogInformation("Subscribing to MQTT topic {Topic}", topic);
		}

		public async Task StopAsync()
		{
			await DisconnectAsync();
		}
	}
}
