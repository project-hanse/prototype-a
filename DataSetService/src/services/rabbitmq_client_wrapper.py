import json
from time import sleep
from typing import Optional

from pika import spec, ConnectionParameters, PlainCredentials
from pika.adapters.blocking_connection import BlockingChannel, BlockingConnection

from src.helper.log_helper import LogHelper
from src.models.dataset_delete_event import DatasetDeleteEvent
from src.services.dataset_store_s3 import DatasetStoreS3


class RabbitMqClientWrapper:

	def __init__(self, dataset_store: DatasetStoreS3) -> None:
		self.channel: Optional[BlockingChannel] = None
		self.connection: Optional[BlockingConnection] = None
		self.topic_name_sub = None
		self.dataset_store = dataset_store
		self.logging = LogHelper.get_logger('RabbitMqClientWrapper')

	def setup(self, client_id: str, host: str, port: int, topic_name_sub: str,
						username: str = None, password: str = None):
		self.logging.info('Connecting to broker at %s:%s' % (host, str(port)))

		self.topic_name_sub = topic_name_sub

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
		self.channel.queue_declare(queue=topic_name_sub, durable=True)
		self.channel.basic_qos(prefetch_count=1)
		self.channel.basic_consume(queue=topic_name_sub, on_message_callback=self.on_message_callback, auto_ack=False)

		self.logging.debug('Connected to broker at %s:%s' % (host, str(port)))

	def cleanup(self):
		if self.connection is not None:
			self.logging.debug('Disconnecting from broker')
			self.connection.close()

	def on_message_callback(self, ch: BlockingChannel, method: spec.Basic.Deliver, properties: spec.BasicProperties,
													body: bytes):
		self.logging.debug("Received message on channel number %s" % str(ch.channel_number))

		try:
			message = body.decode('utf-8')
			payload_deserialized = json.loads(message)
		except Exception as e:
			self.logging.error("Error during deserialization of payload %s" % str(e))
			ch.basic_reject(delivery_tag=method.delivery_tag, requeue=False)
			return

		try:
			request = DatasetDeleteEvent(payload_deserialized)
			self.dataset_store.delete_dataset(request.get_dataset())
		except Exception as e:
			self.logging.error("Error during handling of request %s" % str(e))
			ch.basic_reject(delivery_tag=method.delivery_tag, requeue=True)
			return

		if ch.is_open:
			ch.basic_ack(delivery_tag=method.delivery_tag)
		else:
			self.logging.error("Channel is closed - cannot acknowledge message")
			return

	def start(self):
		self.logging.info('Starting consuming on topic %s' % self.topic_name_sub)
		self.channel.start_consuming()

	def stop(self):
		self.logging.info('Stopping consuming on topic %s' % self.topic_name_sub)
		self.channel.stop_consuming()
