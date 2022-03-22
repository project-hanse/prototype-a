import os

import mlflow
from flask import Flask, render_template, request, jsonify
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO
# Configuration
from mlflow.tracking import MlflowClient

from src.services.dataset_client import DatasetClient
from src.services.model_service import ModelService
from src.services.pipeline_client import PipelineClient
from src.services.trainer_registry import TrainerRegistry

PORT: int = os.getenv("PORT", 5006)
S3_HOST: str = os.getenv("S3_HOST", "localstack")
S3_PORT: str = os.getenv("S3_PORT", "4566")
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "localstack")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "localstack")
MLFLOW_TRACKING_URI: str = os.getenv("MLFLOW_TRACKING_URI", "http://mlflow-server:5005")
MLFLOW_REGISTRY_URI: str = os.getenv("MLFLOW_REGISTRY_URI", "http://mlflow-server:5005")
PIPELINE_SERVICE_HOST: str = os.getenv("PIPELINE_SERVICE_HOST", "pipeline-service")
PIPELINE_SERVICE_PORT: int = os.getenv("PIPELINE_SERVICE_PORT", 5000)
DATASET_SERVICE_HOST: str = os.getenv("DATASET_SERVICE_HOST", "dataset-service")
DATASET_SERVICE_PORT: int = os.getenv("DATASET_SERVICE_PORT", 5002)

# Configure mlflow
os.environ["AWS_ACCESS_KEY_ID"] = S3_ACCESS_KEY_ID
os.environ["AWS_SECRET_ACCESS_KEY"] = S3_ACCESS_KEY_SECRET
os.environ["MLFLOW_S3_ENDPOINT_URL"] = f"http://{S3_HOST}:{S3_PORT}"
mlflow.set_registry_uri(MLFLOW_REGISTRY_URI)
mlflow.set_tracking_uri(MLFLOW_TRACKING_URI)

# Creating service instances
app = Flask(__name__, template_folder='templates')
CORS(app)
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
mlflow_client = MlflowClient(tracking_uri=MLFLOW_TRACKING_URI, registry_uri=MLFLOW_REGISTRY_URI)
pipeline_service_client = PipelineClient(host=PIPELINE_SERVICE_HOST, port=PIPELINE_SERVICE_PORT)
dataset_client = DatasetClient(host=DATASET_SERVICE_HOST, port=DATASET_SERVICE_PORT)
trainer_registry = TrainerRegistry(pipeline_service_client, dataset_client)
model_service = ModelService(mlflow_client, trainer_registry)


# Setting up endpoints
@app.route('/')
@app.route('/index.html')
def root():
	return render_template(
		'index.html',
		data={
			"models": model_service.get_models()
		})


@app.route('/health', methods=['GET'])
def dataframe_by_key():
	if request.method == 'GET':
		return 'OK'


@app.route('/train/<model_name>', methods=['GET'])
def train_model(model_name: str):
	if request.method == 'GET':
		cache_data = request.args.get('cache_data', default=False, type=bool)
		ret = model_service.train_model(model_name=model_name, cache_data=cache_data)
		return jsonify(ret)


@app.route('/predict/<model_name>', methods=['POST'])
def predict(model_name: str):
	if request.method == 'POST':
		data = request.get_json()
		ret = model_service.predict(model_name=model_name, data=data)
		return jsonify(ret)


if __name__ == '__main__':
	socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
# TODO: generate OpenAPI spec https://github.com/marshmallow-code/apispec
