import json
from time import sleep
from typing import Optional

from pika import spec, ConnectionParameters, PlainCredentials
from pika.adapters.blocking_connection import BlockingChannel, BlockingConnection

from src.models.operation_execution_message import OperationExecutionMessage
from src.services.operation_execution_service import OperationExecutionService


class RabbitMqClientWrapper:

	def __init__(self, logging, execution_service: OperationExecutionService) -> None:
		self.channel: Optional[BlockingChannel] = None
		self.connection: Optional[BlockingConnection] = None
		self.topic_name_pub = None
		self.topic_name_sub = None
		self.execution_service = execution_service
		self.logging = logging

	def setup(self, client_id: str, host: str, port: int, topic_name_sub: str, topic_name_pub: str,
						username: str = None, password: str = None):
		self.logging.info('Connecting to broker at %s:%s' % (host, str(port)))

		self.topic_name_sub = topic_name_sub
		self.topic_name_pub = topic_name_pub

		if username is not None and password is not None:
			credentials = PlainCredentials(username, password)
		else:
			credentials = PlainCredentials('guest', 'guest')

		retry_count = 0
		while retry_count < 5:
			try:
				self.connection = BlockingConnection(ConnectionParameters(host=host, port=port,
																																	credentials=credentials,
																																	client_properties={'connection_name': client_id}))
				retry_count = 5
			except Exception as e:
				self.logging.warn("Failed to connect to broker at %s:%s - %s" % (host, str(port), str(e)))
				self.logging.info("Retrying in 5 seconds...")
				sleep(5)
				retry_count += 1

		self.channel = self.connection.channel()
		self.channel.exchange_declare(exchange='direct_messages', exchange_type='direct', durable=True, auto_delete=False)
		self.channel.queue_declare(queue=topic_name_pub, durable=True, auto_delete=False)
		self.channel.queue_declare(queue=topic_name_sub, durable=True, auto_delete=False)
		self.channel.queue_bind(queue=topic_name_pub, exchange='direct_messages', routing_key=topic_name_pub)
		self.channel.queue_bind(queue=topic_name_sub, exchange='direct_messages', routing_key=topic_name_sub)
		self.channel.basic_qos(prefetch_count=1)
		self.channel.basic_consume(queue=topic_name_sub, on_message_callback=self.on_message_callback, auto_ack=False)

		self.logging.debug('Connected to broker at %s:%s' % (host, str(port)))

	def cleanup(self):
		if self.connection is not None and self.connection.is_open:
			self.logging.debug('Disconnecting from broker')
			self.connection.close()

	def on_message_callback(self, ch: BlockingChannel, method: spec.Basic.Deliver, properties: spec.BasicProperties,
													body: bytes):
		self.logging.debug("Received message on channel number '%s' routing_key '%s'" % (str(ch.channel_number), str(method.routing_key)))

		try:
			message = body.decode('utf-8')
			payload_deserialized = json.loads(message)
		except Exception as e:
			self.logging.error("Error during deserialization of payload %s" % str(e))
			ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)
			return

		request = None
		try:
			request = OperationExecutionMessage(payload_deserialized)
			self.logging.info("Operation %s start" % request.get_operation_id())
			response = self.execution_service.handle_execution_request(request)
			self.logging.info("Operation %s complete" % request.get_operation_id())
		except Exception as e:
			if request is not None:
				self.logging.info("Operation %s failed" % request.get_operation_id())
			self.logging.error(
				"Error during handling of request [%s] %s - dropping message (delivery_tag: %s, routing_key: %s, exchange: %s)" % (
					str(type(e)), str(e), str(method.delivery_tag), str(method.routing_key), str(method.exchange)))
			ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)
			return

		if ch.is_open:
			ch.basic_ack(delivery_tag=method.delivery_tag)
		else:
			self.logging.error("Channel is closed - cannot acknowledge message")
			return

		payload_serialized = response.to_json()

		self.logging.debug("Publishing to topic %s with payload %s" % (self.topic_name_pub, payload_serialized))
		ch.basic_publish(exchange='', routing_key=self.topic_name_pub, body=payload_serialized)

	def start(self):
		try:
			self.channel.start_consuming()
		except KeyboardInterrupt as i:
			self.cleanup()
			self.logging.info("Keyboard interrupt received - shutting down")
		except Exception as e:
			self.logging.error("Error during consuming messages - %s" % str(e))

	def stop(self):
		try:
			if self.channel.is_open:
				self.channel.stop_consuming()
		except Exception as e:
			self.logging.warn("Error during stopping of consumer %s" % str(e))
