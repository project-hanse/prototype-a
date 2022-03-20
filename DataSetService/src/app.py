import json
import os

import pandas as pd
from flask import Flask, render_template, request, redirect, abort
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO

from src.constants.metadata_constants import *
from src.helper.response_helper import format_response
from src.services.file_store import FileStore
from src.services.import_service import ImportService
from src.services.in_memory_store import InMemoryStore
from src.services.init_service import InitService

# Configuration
PORT: int = os.getenv("PORT", 5002)
S3_HOST: str = os.getenv("S3_HOST", "localstack")
S3_PORT: str = os.getenv("S3_PORT", "4566")
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "")

# Creating service instances
app = Flask(__name__, template_folder='templates')
CORS(app)
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
dataset_store = InMemoryStore()
file_store = FileStore()
import_service = ImportService(dataset_store)
init_service = InitService(file_store)


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
		data = request.data
		df = pd.read_json(data)
		dataset_store.store_by_key(key, df)
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
		data = request.data
		series = pd.read_json(data, typ='series')
		dataset_store.store_by_key(key, series)
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
		dataset_store.store_by_key(key, data_str)
		return 'OK'


@app.route('/api/metadata/key/<key>', methods=['GET', 'POST'])
def metadata_by_key(key: str):
	metadata_version = METADATA_VERSION_FULL
	if 'version' in request.args:
		metadata_version = request.args['version']
	if request.method == 'GET':
		dataset_store.generate_metadata_by_key(key)
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
			stored = dataset_store.store_metadata_by_key(key, metadata, metadata_version)
			if stored:
				return 'OK'
			return 'Dataset does not exist', 404
		return 'No metadata provided', 400


########################
### Legacy endpoints ###
########################


@app.route('/api/dataframe/<dataset_id>', methods=['GET'])
def dataset_by_id(dataset_id: str):
	"""
	Legacy endpoint. Replaced by GET /api/dataframe/key/<key>?format=json
	"""
	return redirect('/api/dataframe/key/%s?format=json' % dataset_id, 301)


@app.route('/api/dataframe/html/<dataset_id>', methods=['GET'])
def dataset_as_html_by_id(dataset_id: str):
	"""
	Legacy endpoint. Replaced by GET /api/dataframe/key/<key>?format=html
	"""
	return redirect('/api/dataframe/key/%s?format=html' % dataset_id, 301)


@app.route('/api/dataframe/csv/<dataset_id>', methods=['GET'])
def dataset_as_csv_by_id(dataset_id: str):
	"""
	Legacy endpoint. Will be replaced by GET /api/dataframe/key/<key>?format=csv
	"""
	return redirect('/api/dataframe/key/%s?format=csv' % dataset_id, 301)


@app.route('/api/dataframe/key/describe/<key>', methods=['GET'])
def describe_dataset_by_key(key: str):
	"""
	Legacy endpoint. Replaced by GET /api/metadata/key/<key>?format=json
	"""
	return redirect('/api/metadata/key/%s?format=json' % key, 301)


@app.route('/api/dataframe/key/describe/html/<key>', methods=['GET'])
def describe_dataframe_by_key_html(key: str):
	"""
	Legacy endpoint. Replaced by GET /api/metadata/key/<key>?format=html
	"""
	return redirect('/api/metadata/key/%s?format=html' % key, 301)


# Initializing services
file_store.setup(s3_endpoint=("http://%s:%s" % (S3_HOST, S3_PORT)),
								 s3_access_key_id=S3_ACCESS_KEY_ID,
								 s3_secret_access_key=S3_ACCESS_KEY_SECRET)
init_service.init_default_files_s3()
import_service.import_defaults_in_background()

if __name__ == '__main__':
	socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
# TODO: generate OpenAPI spec https://github.com/marshmallow-code/apispec
