import os

from flask import Flask, render_template, request
from flask_bootstrap import Bootstrap
from flask_cors import CORS
from flask_socketio import SocketIO

# Configuration
PORT: int = os.getenv("PORT", 5006)
S3_HOST: str = os.getenv("S3_HOST", "localstack")
S3_PORT: str = os.getenv("S3_PORT", "4566")
S3_ACCESS_KEY_SECRET: str = os.getenv("S3_ACCESS_KEY_SECRET", "")
S3_ACCESS_KEY_ID: str = os.getenv("S3_ACCESS_KEY_ID", "")

# Creating service instances
app = Flask(__name__, template_folder='templates')
CORS(app)
socketio = SocketIO(app)
bootstrap = Bootstrap(app)


# Setting up endpoints
@app.route('/')
@app.route('/index.html')
def root():
	return render_template(
		'index.html',
		data={})


@app.route('/health', methods=['GET'])
def dataframe_by_key():
	if request.method == 'GET':
		return 'OK'


if __name__ == '__main__':
	socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
# TODO: generate OpenAPI spec https://github.com/marshmallow-code/apispec
