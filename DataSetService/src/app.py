import io
import json
import os
import signal
import threading
import uuid

import pandas as pd
from flask import Flask, render_template, request, abort
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO

from src.constants.metadata_constants import *
from src.helper.log_helper import LogHelper
from src.helper.response_helper import format_response
from src.services.dataset_store_s3 import DatasetStoreS3
from src.services.file_store_s3 import FileStoreS3
from src.services.init_service import InitService
from src.services.rabbitmq_client_wrapper import RabbitMqClientWrapper

# Configuration
PORT: int = os.getenv("PORT", 5002)
S3_HOST: str = os.getenv("S3_HOST", "localstack")
S3_PORT: str = os.getenv("S3_PORT", "4566")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "")
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "")
S3_REGION: str = os.getenv("S3_REGION", "eu-west-3")
MESSAGE_BROKER_HOST: str = os.getenv("MESSAGE_BROKER_HOST", "rabbitmq")
MESSAGE_BROKER_PORT: int = os.getenv("MESSAGE_BROKER_PORT", 5672)
MESSAGE_BROKER_TOPIC_DELETE = os.getenv("MESSAGE_BROKER_TOPIC_DELETE", "dataset/delete")
CLIENT_ID: str = os.getenv("MESSAGE_BROKER_CLIENT_ID", ("dataset-service-" + str(uuid.uuid4())))

# Creating service instances
app = Flask(__name__, template_folder='templates')
CORS(app)
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
file_store = FileStoreS3()
init_service = InitService(file_store)
dataset_store = DatasetStoreS3()
event_bus_wrapper = RabbitMqClientWrapper(dataset_store)
log = LogHelper.get_logger(__name__)

# Setup Threads
event_bus_thread = threading.Thread(target=event_bus_wrapper.start)
flask_app_thread = threading.Thread(target=socketio.run,
																		args=(app,),
																		kwargs={"host": '0.0.0.0',
																						"port": PORT,
																						"use_reloader": False,
																						"debug": True,
																						"allow_unsafe_werkzeug": True})


# Setup signal handling
def signal_handler(sig, frame):
	if event_bus_thread.is_alive():
		event_bus_wrapper.stop()
	dataset_store.stop()
	os.kill(os.getpid(), signal.SIGTERM)


signal.signal(signal.SIGINT, signal_handler)


# Setting up endpoints
@app.route('/')
@app.route('/index.html')
def root():
	return render_template(
		'index.html',
		data={
			'dataset_count': dataset_store.get_dataset_count()
		})


@app.route('/api/dataframe/key/<key>', methods=['GET', 'POST'])
def dataframe_by_key(key: str):
	if request.method == 'GET':
		df = dataset_store.get_by_key(key, pd.DataFrame)

		if df is None:
			abort(404)
		requested_format = request.args.get('format')
		return format_response(df, requested_format)

	if request.method == 'POST':
		df = pd.read_json(io.BytesIO(request.data))
		dataset_store.store_data_by_key(key, df)
		return 'OK'


@app.route('/api/series/key/<key>', methods=['GET', 'POST'])
def series_by_key(key: str):
	if request.method == 'GET':
		series = dataset_store.get_by_key(key, pd.Series)

		if series is None:
			abort(404)
		requested_format = request.args.get('format')
		return format_response(series, requested_format)
	if request.method == 'POST':
		data = io.BytesIO(request.data)
		series = pd.read_json(data, typ='series')
		dataset_store.store_data_by_key(key, series)
		return 'OK'


@app.route('/api/string/key/<key>', methods=['GET', 'POST'])
def string_by_key(key: str):
	if request.method == 'GET':
		data = dataset_store.get_by_key(key, str)

		if data is None:
			abort(404)

		return data

	if request.method == 'POST':
		data = request.data
		data_str: str = data.decode('utf-8')
		dataset_store.store_data_by_key(key, data_str)
		return 'OK'


@app.route('/api/metadata/key/<key>', methods=['GET', 'POST'])
def metadata_by_key(key: str):
	metadata_version = METADATA_VERSION_FULL
	if 'version' in request.args:
		metadata_version = request.args['version']
	if request.method == 'GET':
		metadata = dataset_store.get_metadata_by_key(key, metadata_version)

		if metadata is None:
			abort(404)

		requested_format = request.args.get('format')
		return format_response(metadata, requested_format)
	if request.method == 'POST':
		data_format = request.args.get('format')
		if data_format is None or data_format == 'json':
			metadata = json.loads(request.data.decode('utf-8'))
		else:
			return 'Unsupported format', 400
		if metadata is not None:
			stored = dataset_store.extend_metadata_by_key(key, metadata, metadata_version)
			if stored:
				return 'OK'
			return 'Dataset does not exist', 404
		return 'No metadata provided', 400


# Initializing services
dataset_store.setup(s3_endpoint=("http://%s:%s" % (S3_HOST, S3_PORT)),
										s3_access_key_id=S3_ACCESS_KEY_ID,
										s3_access_key_secret=S3_ACCESS_KEY_SECRET,
										s3_region=S3_REGION)
file_store.setup(s3_endpoint=("http://%s:%s" % (S3_HOST, S3_PORT)),
								 s3_access_key_id=S3_ACCESS_KEY_ID,
								 s3_access_key_secret=S3_ACCESS_KEY_SECRET,
								 s3_region=S3_REGION)

init_service.init_default_files_s3_in_background()
event_bus_wrapper.setup(CLIENT_ID, MESSAGE_BROKER_HOST, MESSAGE_BROKER_PORT, MESSAGE_BROKER_TOPIC_DELETE)

if __name__ == '__main__':
	log.info("Starting event bus thread in background")
	event_bus_thread.start()
	log.info("Starting Flask app")
	flask_app_thread.start()
	# TODO: generate OpenAPI spec https://github.com/marshmallow-code/apispec

	log.info("Shutting down...")
	log.info("Waiting for flask process to finish")
	flask_app_thread.join()
	log.info("Waiting for event bus thread to finish...")
	event_bus_thread.join()
	log.info("Shutting down complete.")
