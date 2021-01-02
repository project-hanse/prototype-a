import signal
import time
import uuid

import paho.mqtt.client as mqtt

MQTT_HOST: str = "localhost"
MQTT_PORT: int = 1883
TOPIC_NAME: str = "execute/+"
CLIENT_ID: str = "BlockWorker-" + str(uuid.uuid4())


def on_connect(client, userdata, flags, rc):
    print("Connected with result code %s" % str(rc))
    # Subscribing in on_connect() means that if we lose the connection and reconnect then subscriptions will be renewed.
    print("Subscribing to topic " + TOPIC_NAME)
    client.subscribe(TOPIC_NAME)


def on_message(client, userdata, msg):
    print("Payload %s on topic client %s" % (str(msg.payload), str(msg.topic)))


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
