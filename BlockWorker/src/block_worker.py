import signal
import time
import uuid
import os
import json

import paho.mqtt.client as mqtt

MQTT_HOST: str = os.getenv("MQTT_HOST", "message-broker")
MQTT_PORT: int = os.getenv("MQTT_PORT", 1883)
CLIENT_ID: str = os.getenv("MQTT_CLIENT_ID", ("BlockWorker-" + str(uuid.uuid4())))
TOPIC_NAME_SUB: str = os.getenv("MQTT_TOPIC_SUB", "execute/+")
TOPIC_NAME_PUB: str = os.getenv("MQTT_TOPIC_PUB", "executed")


def on_connect(client, userdata, flags, rc):
    print("Connected with result code %s" % str(rc))
    # Subscribing in on_connect() means that if we lose the connection and reconnect then subscriptions will be renewed.
    print("Subscribing to topic " + TOPIC_NAME_SUB)
    client.subscribe(TOPIC_NAME_SUB)


def on_message(client, userdata, msg):
    print("Payload %s on topic client %s" % (str(msg.payload), str(msg.topic)))
    payload = json.loads(msg.payload)
    # TODO make this more robust (error handling,...)
    response = {
        'PipelineId': payload['PipelineId'],
        'BlockId': payload['BlockId'],
        'Successful': True,
        'ExecutionTime': 0,
        'ResultDatasetId': str(uuid.uuid4())
    }

    topic = ("%s/%s/%s" % (TOPIC_NAME_PUB, payload['PipelineId'], payload['ExecutionId']))
    payload_serialized = json.dumps(response)

    print("Publishing to topic %s with payload %s" % (topic, payload_serialized))

    client.publish(topic, payload=payload_serialized)


class Program:
    _running = True
    _mqtt_client = None

    def __init__(self):
        self._mqtt_client = mqtt.Client(client_id=CLIENT_ID, clean_session=True)
        self._mqtt_client.on_connect = on_connect
        self._mqtt_client.on_message = on_message

    def run(self):
        print("Connecting to broker at %s:%s " % (MQTT_HOST, str(MQTT_PORT)))
        self._mqtt_client.connect(MQTT_HOST, MQTT_PORT, 60)

        while self._running:
            self._mqtt_client.loop()
            time.sleep(1)

        print("Disconnecting from MQTT")
        self._mqtt_client.disconnect()

    def stop(self):
        self._running = False

    def __del__(self):
        del self._mqtt_client


program = Program()


def sigterm_handler(_signo, _stack_frame):
    print("\nGot signal %s" % str(_signo))
    program.stop()


signal.signal(signal.SIGTERM, sigterm_handler)
signal.signal(signal.SIGINT, sigterm_handler)

program.run()

print("[end of program]")
