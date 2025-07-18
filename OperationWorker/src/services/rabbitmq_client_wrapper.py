import datetime
import json
from time import sleep
from typing import Optional

from pika import spec, ConnectionParameters, PlainCredentials
from pika.adapters.blocking_connection import BlockingChannel, BlockingConnection

from src.models.operation_executed_message import OperationExecutedMessage
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
		self.logging.info(
			"Received message on channel number '%s' routing_key '%s'" % (str(ch.channel_number), str(method.routing_key)))

		try:
			message = body.decode('utf-8')
			payload_deserialized = json.loads(message)
		except Exception as e:
			self.logging.error("Error during deserialization of payload %s" % str(e))
			ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)
			return

		operation_start_time = datetime.datetime.now(datetime.timezone.utc)
		op_id = "unknown"
		try:
			self.logging.info("Constructing operation execution message")
			request = OperationExecutionMessage(payload_deserialized)
			op_id = request.get_operation_id() if request is not None else "unknown"
			self.logging.info("Operation %s start at %s" % (op_id, str(operation_start_time)))
			response = self.execution_service.handle_execution_request(request)
		except Exception as e:
			self.logging.error(
				"Error during handling of request [%s] %s - dropping message (delivery_tag: %s, routing_key: %s, exchange: %s, operation_id: '%s')" % (
					str(type(e)), str(e), str(method.delivery_tag), str(method.routing_key), str(method.exchange), op_id))
			response = OperationExecutedMessage()
			if payload_deserialized is not None:
				if "OperationId" in payload_deserialized:
					response.operation_id = payload_deserialized["OperationId"]
				if "ExecutionId" in payload_deserialized:
					response.execution_id = payload_deserialized["ExecutionId"]
			response.successful = False
			response.error_message = str(e)
		finally:
			operation_end_time = datetime.datetime.now(datetime.timezone.utc)
			operation_duration = operation_end_time - operation_start_time
			self.logging.info("Operation '%s' complete in %s" % (op_id, str(operation_duration)))

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
