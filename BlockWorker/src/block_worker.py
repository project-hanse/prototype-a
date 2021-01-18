import signal
import time
import uuid
import os
import json
import random
import datetime

import paho.mqtt.client as mqtt

MQTT_HOST: str = os.getenv("MQTT_HOST", "message-broker")
MQTT_PORT: int = os.getenv("MQTT_PORT", 1883)
CLIENT_ID: str = os.getenv("MQTT_CLIENT_ID", ("BlockWorker-" + str(uuid.uuid4())))
TOPIC_NAME_SUB: str = os.getenv("MQTT_TOPIC_SUB", "execute/+")
TOPIC_NAME_PUB: str = os.getenv("MQTT_TOPIC_PUB", "executed")


def connect_callback(client, userdata, flags, reasonCode, properties=None):
    print("Connected with code %s" % str(reasonCode))
    # Subscribing in on_connect() means that if we lose the connection and reconnect then subscriptions will be renewed.
    print("Subscribing to topic " + TOPIC_NAME_SUB)
    client.subscribe(TOPIC_NAME_SUB)


def on_message_callback(client, userdata, message):
    payload = json.loads(message.payload)

    print("Received message on topic %s:\n%s" % (
        str(message.topic), json.dumps(payload, indent=2, sort_keys=True, default=str)))

    # TODO make this more robust (error handling,...)

    response = {
        'PipelineId': payload['PipelineId'],
        'BlockId': payload['BlockId'],
        'ExecutionId': payload['ExecutionId'],
        'Successful': True,
        'StartTime': datetime.datetime.now(datetime.timezone.utc),
        'ResultDatasetId': str(uuid.uuid4())
    }

    rand = random.randint(1, 6)
    print("Simulating %s for %i seconds..." % (payload['OperationName'], rand))
    time.sleep(rand)

    response['StopTime'] = datetime.datetime.now(datetime.timezone.utc)
    response['ResultDatasetId'] = None

    topic = ("%s/%s/%s" % (TOPIC_NAME_PUB, payload['PipelineId'], payload['ExecutionId']))
    payload_serialized = json.dumps(response, indent=2, sort_keys=True, default=str)

    print("Publishing to topic %s with payload %s" % (topic, payload_serialized))

    client.publish(topic, payload=payload_serialized, qos=0)


def on_publish_callback(client, userdata, mid):
    print("Published message %i" % mid)
    print()


class Program:
    _running = True
    _mqtt_client = None

    def __init__(self):
        self._mqtt_client = mqtt.Client(client_id=CLIENT_ID, clean_session=False)
        self._mqtt_client.on_connect = connect_callback
        self._mqtt_client.on_message = on_message_callback
        self._mqtt_client.on_publish = on_publish_callback

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
