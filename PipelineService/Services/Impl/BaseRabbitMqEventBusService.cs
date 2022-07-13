using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PipelineService.Models.MqttMessages;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace PipelineService.Services.Impl
{
	public abstract class BaseRabbitMqEventBusService : IEventBusService
	{
		private readonly ILogger<BaseRabbitMqEventBusService> _logger;
		private ConnectionFactory _factory;
		private IConnection _connection;
		private readonly IDictionary<string, IModel> _channels;

		protected abstract string Hostname { get; }
		protected abstract int Port { get; }
		protected abstract string ClientId { get; }
		protected abstract string Username { get; }
		protected abstract string Password { get; }

		protected BaseRabbitMqEventBusService(ILogger<BaseRabbitMqEventBusService> logger)
		{
			_logger = logger;
			_channels = new Dictionary<string, IModel>();
		}

		protected async Task ConnectAsync()
		{
			if (_connection != null && _connection.IsOpen)
			{
				_logger.LogDebug("Already connected to message broker {Hostname}:{Port}", Hostname, Port);
				return;
			}

			_logger.LogDebug("Connecting to message broker {Hostname}:{Port}", Hostname, Port);

			_factory = new ConnectionFactory()
			{
				HostName = Hostname,
				Port = Port,
				UserName = Username,
				Password = Password,
				ClientProvidedName = ClientId,
				DispatchConsumersAsync = true
			};

			var retryCounter = 0;
			while (retryCounter < 5)
			{
				try
				{
					_connection = _factory.CreateConnection();
					retryCounter = 5;
				}
				catch (BrokerUnreachableException e)
				{
					_logger.LogDebug("Failed to connect to message broker {Hostname}:{Port} - {Reason}", Hostname, Port, e.Message);
					_logger.LogWarning("Retrying to connect to message broker {Hostname}:{Port}", Hostname, Port);
					await Task.Delay(5000);
					retryCounter++;
				}
			}

			_logger.LogInformation("Connected to message broker {Hostname}:{Port}", Hostname, Port);
		}

		protected async Task<IModel> GetChannel(string queueName)
		{
			await ConnectAsync();

			if (_channels.TryGetValue(queueName, out var channel))
			{
				return channel;
			}

			_logger.LogDebug("Creating channel for queue {QueueName}", queueName);

			channel = _connection.CreateModel();

			channel.QueueDeclare(
				queue: queueName,
				durable: true,
				exclusive: false,
				autoDelete: false,
				arguments: null
			);

			// only deliver one message at a time to the consumer (worker) instead of round-robin
			channel.BasicQos(0, 1, false);

			_channels.Add(queueName, channel);

			_logger.LogInformation("Created channel for queue {QueueName}", queueName);

			return channel;
		}

		protected Task DisconnectAsync()
		{
			if (_connection == null || !_connection.IsOpen || _factory == null)
			{
				_logger.LogDebug("Already disconnected from message broker {Hostname}:{Port}", Hostname, Port);
				return Task.CompletedTask;
			}

			_logger.LogDebug("Disconnecting from message broker {Hostname}:{Port}", Hostname, Port);

			// also closes all channels
			_connection.Close();
			_connection.Dispose();
			_connection = null;
			_factory = null;

			_logger.LogInformation("Disconnected from message broker {Hostname}:{Port}", Hostname, Port);

			return Task.CompletedTask;
		}

		public abstract Task PublishMessage<T>(string topic, T payload) where T : BaseMqttMessage;

		public abstract Task Subscribe<T>(string topic, Func<T, Task> handler) where T : BaseMqttMessage;
	}
}
