import logging
import os

from flask import Flask, render_template, abort, redirect, jsonify
from flask_bootstrap import Bootstrap
from flask_socketio import SocketIO

from services.in_memory_store import InMemoryStore

PORT: int = os.getenv("PORT", 5000)

logging.basicConfig(
    level=logging.DEBUG,
    format="%(asctime)s [%(levelname)s] %(message)s",
    handlers=[
        logging.StreamHandler()
    ]
)
app = Flask(__name__, template_folder='templates')
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
store = InMemoryStore(logging)


@app.route('/')
def hello():
    return redirect("/index.html", code=302)


@app.route('/index.html')
def root():
    return render_template(
        'index.html',
        data={
            'operations_count': store.get_operation_count()
        })


@app.route('/api/operations/<operation_id>', methods=['GET'])
def dataset_by_id(operation_id: str):
    response = {
        "operationId": operation_id,
        "serializedOperation": store.get_by_id(operation_id).hex()
    }

    if response["serializedOperation"] is None:
        abort(404)

    return jsonify(response)


# Generating default operations
store.generate_defaults()

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
