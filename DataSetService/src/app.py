import os

import pandas as pd
from flask import Flask, render_template, request, abort
from flask_bootstrap import Bootstrap
from flask_jsonpify import jsonpify
from flask_socketio import SocketIO

from services.in_memory_store import InMemoryStore

PORT: int = os.getenv("PORT", 5000)

app = Flask(__name__, template_folder='templates')
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
store = InMemoryStore()


@app.route('/index.html')
def root():
    return render_template(
        'index.html',
        data={
            'dataset_count': store.get_dataset_count()
        })


@app.route('/api/datasets/<dataset_id>', methods=['GET'])
def dataset_by_id(dataset_id: str):
    df = store.get_by_id(dataset_id)

    if df is None:
        abort(404)

    df_list = df.values.tolist()
    return jsonpify(df_list)


@app.route('/api/datasets/hash/<producing_hash>', methods=['GET', 'POST'])
def dataset_by_hash(producing_hash: str):
    if request.method == 'GET':
        df = store.get_by_hash(producing_hash)

        if df is None:
            abort(404)

        df_list = df.values.tolist()
        return jsonpify(df_list)

    if request.method == 'POST':
        data = request.data
        df = pd.read_json(data)
        store.store_data_set(producing_hash, df)
        return 'OK'


store.import_with_id("../datasets/Melbourne_housing_FULL.csv", "00e61417-cada-46db-adf3-a5fc89a3b6ee")
store.import_with_id("../datasets/MELBOURNE_HOUSE_PRICES_LESS.csv", "0c2acbdb-544b-4efc-ae54-c2dcba988654")

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
