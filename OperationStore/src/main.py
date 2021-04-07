import logging
import os

from flask import Flask, render_template, abort, redirect
from flask_bootstrap import Bootstrap
from flask_jsonpify import jsonpify
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
    df = store.get_by_id(operation_id)

    if df is None:
        abort(404)

    df_list = df.values.tolist()
    return jsonpify(df_list)


# Generating default operations
store.generate_defaults()

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
