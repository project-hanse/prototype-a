import logging
import os
import signal
import uuid

from src.services.dateset_service_client import DatasetServiceClient
from src.services.file_store_client import FileStoreClient
from src.services.mqtt_client_wrapper import MqttClientWrapper
from src.services.node_execution_service import NodeExecutionService
from src.services.operation_service import OperationService

MQTT_HOST: str = os.getenv("MQTT_HOST", "message-broker")
MQTT_PORT: int = os.getenv("MQTT_PORT", 1883)
CLIENT_ID: str = os.getenv("MQTT_CLIENT_ID", ("OperationWorker-" + str(uuid.uuid4())))
TOPIC_NAME_SUB: str = os.getenv("MQTT_TOPIC_SUB", "execute/+/+")
TOPIC_NAME_PUB: str = os.getenv("MQTT_TOPIC_PUB", "executed")
DATASET_HOST: str = os.getenv("DATASET_HOST", "dataset-service")
DATASET_PORT: int = os.getenv("DATASET_PORT", 5002)
S3_HOST: str = os.getenv("S3_HOST", "http://localstack-s3")
S3_PORT: int = os.getenv("S3_PORT", 4566)
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "")

logging.basicConfig(
	level=logging.DEBUG,
	format="%(asctime)s [%(levelname)s] %(message)s",
	handlers=[
		logging.StreamHandler()
	]
)

loop = True


def sigterm_handler(_signo, _stack_frame):
	logging.debug("Got signal %s" % str(_signo))
	global loop
	loop = False


if __name__ == '__main__':
	logging.info('Starting Operation Worker')
	operation_service = OperationService(logging)
	dataset_client = DatasetServiceClient(DATASET_HOST, DATASET_PORT, logging)
	file_store_client = FileStoreClient(logging)
	node_execution_service = NodeExecutionService(logging, dataset_client, file_store_client, operation_service)
	client_wrapper = MqttClientWrapper(logging, node_execution_service)

	logging.debug('Setting up signal handler')
	signal.signal(signal.SIGTERM, sigterm_handler)
	signal.signal(signal.SIGINT, sigterm_handler)

	client_wrapper.setup(client_id=CLIENT_ID,
											 mqtt_host=MQTT_HOST,
											 mqtt_port=MQTT_PORT,
											 topic_name_sub=TOPIC_NAME_SUB,
											 topic_name_pub=TOPIC_NAME_PUB)
	file_store_ok = file_store_client.setup(s3_endpoint=("%s:%i" % (S3_HOST, S3_PORT)),
																					s3_access_key_id=S3_ACCESS_KEY_ID,
																					s3_secret_access_key=S3_ACCESS_KEY_SECRET)
	if not file_store_ok:
		raise Exception("Failed to setup file store")

	operation_service.init()

	while loop:
		client_wrapper.loop()

	logging.info('Shutting down')

	try:
		client_wrapper.cleanup()
	except Exception as e:
		logging.exception('Error during shutdown', e)
		exit(1)

	exit(0)
