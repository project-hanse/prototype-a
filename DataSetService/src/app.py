import os

import pandas as pd
from flask import Flask, render_template, request, abort
from flask_bootstrap import Bootstrap
from flask_socketio import SocketIO

from src.services.import_service import ImportService
from src.services.in_memory_store import InMemoryStore

PORT: int = os.getenv("PORT", 5000)

app = Flask(__name__, template_folder='templates')
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
store = InMemoryStore()
import_service = ImportService(store)


@app.route('/')
@app.route('/index.html')
def root():
    return render_template(
        'index.html',
        data={
            'dataset_count': store.get_dataset_count(),
            'dataset_ids': store.get_ids()
        })


@app.route('/api/datasets/<dataset_id>', methods=['GET'])
def dataset_by_id(dataset_id: str):
    df = store.get_by_id(dataset_id)

    if df is None:
        abort(404)

    return my_jsonpify(df)


@app.route('/api/datasets/html/<dataset_id>', methods=['GET'])
def dataset_as_html_by_id(dataset_id: str):
    df = store.get_by_id(dataset_id)

    if df is None:
        abort(404)

    return df.to_html()


@app.route('/api/datasets/hash/<producing_hash>', methods=['GET', 'POST'])
def dataset_by_hash(producing_hash: str):
    if request.method == 'GET':
        df = store.get_by_hash(producing_hash)

        if df is None:
            abort(404)

        return my_jsonpify(df)

    if request.method == 'POST':
        data = request.data
        df = pd.read_json(data)
        store.store_data_set(producing_hash, df)
        return 'OK'


def my_jsonpify(df):
    return app.response_class(
        response=df.to_json(),
        status=200,
        mimetype='application/json'
    )


import_service.import_defaults_in_background()

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
