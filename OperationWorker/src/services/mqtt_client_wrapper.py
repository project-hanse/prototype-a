import json

import paho.mqtt.client as mqtt

from src.models.operation_execution_message import OperationExecutionMessage
from src.services.node_execution_service import NodeExecutionService


class MqttClientWrapper:
	logging = None
	execution_service: NodeExecutionService
	client: mqtt.Client
	topic_name_sub: str
	topic_name_pub: str

	def __init__(self, logging, execution_service: NodeExecutionService) -> None:
		self.execution_service = execution_service
		self.logging = logging

	def setup(self, client_id: str, mqtt_host: str, mqtt_port: int, topic_name_sub: str, topic_name_pub: str):
		self.logging.info('Connecting to broker at %s:%s' % (mqtt_host, str(mqtt_port)))

		self.topic_name_sub = topic_name_sub
		self.topic_name_pub = topic_name_pub

		self.client = mqtt.Client(client_id=client_id, clean_session=False)
		self.client.on_connect = self.connect_callback
		self.client.on_message = self.on_message_callback
		self.client.on_publish = self.on_publish_callback
		self.client.connect(mqtt_host, port=mqtt_port)
		self.logging.debug('Broker client setup complete')

	def loop(self):
		self.client.loop()

	def cleanup(self):
		if self.client is not None:
			self.logging.debug('Disconnecting from broker')
			self.client.disconnect()

	def connect_callback(self, client, userdata, flags, reason_code, properties=None):
		self.logging.debug("Connected with code %s" % str(reason_code))
		self.logging.info("Subscribing to topic %s" % self.topic_name_sub)
		client.subscribe(self.topic_name_sub)

	def on_message_callback(self, client, userdata, message):
		# TODO make this more robust (error handling, MQTT service level 2 ...)
		self.logging.debug("Received message on topic %s:\n%s" % (str(message.topic), message.payload))

		try:
			payload_deserialized = json.loads(message.payload)
		except Exception as e:
			self.logging.error("Error during deserialization of payload %s" % str(e))
			self.publish_error_message(str(e))
			return

		# TODO this contains hardcoded strings that might change in the config (MQTT_TOPIC_SUB);
		#  - Possible solution: validate at startup?
		if message.topic.endswith("file"):
			request = OperationExecutionMessage(payload_deserialized)
			response = self.execution_service.handle_file_input_request(request)
		elif message.topic.endswith("single"):
			request = OperationExecutionMessage(payload_deserialized)
			response = self.execution_service.handle_single_input_request(request)
		elif message.topic.endswith("double"):
			request = OperationExecutionMessage(payload_deserialized)
			response = self.execution_service.handle_double_input_request(request)
		else:
			self.logging.error("Got message that can not be processed by this worker")
			self.publish_error_message("Unknown message")
			# TODO: requeue message
			return

		topic = ("%s/%s/%s" % (self.topic_name_pub, request.get_pipeline_id(), request.get_pipeline_id()))

		payload_serialized = response.to_json()

		self.logging.debug("Publishing to topic %s with payload %s" % (topic, payload_serialized))

		client.publish(topic, payload=payload_serialized, qos=2)

	def publish_error_message(self, message: str):
		self.logging.debug("Publishing error '%s' to event bus" % message)
		# TODO: implement me

	def on_publish_callback(self, client, userdata, mid):
		self.logging.debug("Published message %i" % mid)
