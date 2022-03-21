import os

from flask import Flask, render_template, request
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO
# Configuration
from mlflow.tracking import MlflowClient

from src.services.model_service import ModelService

PORT: int = os.getenv("PORT", 5006)
S3_HOST: str = os.getenv("S3_HOST", "localstack")
S3_PORT: str = os.getenv("S3_PORT", "4566")
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "")
MLFLOW_TRACKING_URI: str = os.getenv("MLFLOW_TRACKING_URI", "http://mlflow-server:5005")
MLFLOW_REGISTRY_URI: str = os.getenv("MLFLOW_REGISTRY_URI", "http://mlflow-server:5005")

# Set environment variables for boto3
os.environ["AWS_ACCESS_KEY_ID"] = S3_ACCESS_KEY_ID
os.environ["AWS_SECRET_ACCESS_KEY"] = S3_ACCESS_KEY_SECRET
os.environ["MLFLOW_S3_ENDPOINT_URL"] = f"http://{S3_HOST}:{S3_PORT}"

# Creating service instances
app = Flask(__name__, template_folder='templates')
CORS(app)
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
mlflow_client = MlflowClient(tracking_uri=MLFLOW_TRACKING_URI, registry_uri=MLFLOW_REGISTRY_URI)
model_service = ModelService(mlflow_client)


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


if __name__ == '__main__':
	socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
# TODO: generate OpenAPI spec https://github.com/marshmallow-code/apispec
