import os

import pandas as pd
from flask import Flask, render_template, request, abort
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO

from src.services.file_store import FileStore
from src.services.import_service import ImportService
from src.services.in_memory_store import InMemoryStore
from src.services.init_service import InitService

# Configuration
PORT: int = os.getenv("PORT", 5000)
S3_HOST: str = os.getenv("S3_HOST", "localstack-s3")
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


# Setting up endpoint

@app.route('/')
@app.route('/index.html')
def root():
	return render_template(
		'index.html',
		data={
			'dataset_count': dataset_store.get_dataset_count(),
			'dataset_ids': dataset_store.get_ids()
		})


@app.route('/api/dataframe/<dataset_id>', methods=['GET'])
def dataset_by_id(dataset_id: str):
	df = dataset_store.get_by_id(dataset_id)

	if df is None:
		abort(404)

	return my_jsonpify(df)


@app.route('/api/dataframe/html/<dataset_id>', methods=['GET'])
def dataset_as_html_by_id(dataset_id: str):
	df = dataset_store.get_by_id(dataset_id)

	if df is None:
		abort(404)

	return df.to_html()


@app.route('/api/dataframe/csv/<dataset_id>', methods=['GET'])
def dataset_as_csv_by_id(dataset_id: str):
	df = dataset_store.get_by_id(dataset_id)

	if df is None:
		abort(404)

	return df.to_csv()


@app.route('/api/dataframe/key/<key>', methods=['GET', 'POST'])
def dataframe_by_key(key: str):
	if request.method == 'GET':
		df = dataset_store.get_by_key(key)

		if df is None:
			abort(404)

		return my_jsonpify(df)

	if request.method == 'POST':
		data = request.data
		df = pd.read_json(data)
		dataset_store.store_dataframe_by_key(key, df)
		return 'OK'


@app.route('/api/string/key/<key>', methods=['GET', 'POST'])
def string_by_key(key: str):
	if request.method == 'GET':
		data = dataset_store.get_by_key(key)

		if data is None:
			abort(404)

		return data

	if request.method == 'POST':
		data = request.data
		dataset_store.store_by_key(key, data)
		return 'OK'


@app.route('/api/dataframe/key/describe/<key>', methods=['GET'])
def describe_dataset_by_hash(key: str):
	if request.method == 'GET':
		df = dataset_store.get_by_key(key)

		if df is None:
			abort(404)

		return my_jsonpify(df.describe())


@app.route('/api/dataframe/key/describe/html/<key>', methods=['GET'])
def describe_dataframe_by_key_html(key: str):
	if request.method == 'GET':
		df = dataset_store.get_by_key(key)

		if df is None:
			abort(404)

		return df.describe().to_html()


def my_jsonpify(df):
	return app.response_class(
		response=df.to_json(date_format='iso'),
		status=200,
		mimetype='application/json'
	)


# Initializing services
file_store.setup(s3_endpoint=("http://%s:%s" % (S3_HOST, S3_PORT)),
								 s3_access_key_id=S3_ACCESS_KEY_ID,
								 s3_secret_access_key=S3_ACCESS_KEY_SECRET)
init_service.init_default_files_s3()
import_service.import_defaults_in_background()

if __name__ == '__main__':
	socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
