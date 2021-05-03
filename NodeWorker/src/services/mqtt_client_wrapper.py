import json

import paho.mqtt.client as mqtt

from src.models.simple_node_execution_request import SimpleNodeExecutionRequest
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
        request = SimpleNodeExecutionRequest(json.loads(message.payload))

        self.logging.debug("Received message on topic %s:\n%s" % (str(message.topic), request.to_json()))

        response = self.execution_service.handle_simple_request(request)

        topic = ("%s/%s/%s" % (self.topic_name_pub, request.get_pipeline_id(), request.get_pipeline_id()))

        payload_serialized = response.to_json()

        self.logging.debug("Publishing to topic %s with payload %s" % (topic, payload_serialized))

        client.publish(topic, payload=payload_serialized, qos=0)

    def on_publish_callback(self, client, userdata, mid):
        self.logging.debug("Published message %i" % mid)
