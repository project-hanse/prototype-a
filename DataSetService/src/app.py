import os

import pandas as pd
from flask import Flask, render_template, request, abort
from flask_bootstrap import Bootstrap
from flask_socketio import SocketIO

from services.in_memory_store import InMemoryStore

PORT: int = os.getenv("PORT", 5000)

app = Flask(__name__, template_folder='templates')
socketio = SocketIO(app)
bootstrap = Bootstrap(app)
store = InMemoryStore()


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


# Importing default datasets
store.import_with_id("../datasets/Melbourne_housing_FULL.csv", "00e61417-cada-46db-adf3-a5fc89a3b6ee")
store.import_with_id("../datasets/MELBOURNE_HOUSE_PRICES_LESS.csv", "0c2acbdb-544b-4efc-ae54-c2dcba988654")
store.import_with_id("../datasets/influenca_vienna_2009-2018.csv", "4cfd0698-004a-404e-8605-de2f830190f2")
store.import_with_id("../datasets/weather_vienna_2009-2018.csv", "244b5f61-1823-48fb-b7fa-47a2699bb580")
store.import_with_id("../datasets/21211-003Z_format.csv", "2b88720f-8d2d-46c8-84d2-ab177c88cb5f")
store.import_with_id("../datasets/21311-001Z_format.csv", "61501213-d945-49a5-9212-506d6305af13")
store.import_with_id("../datasets/simulated-vine-yield-styria.xlsx", "1a953cb2-4ad1-4c07-9a80-bd2c6a68623a")

# Load ZAMG data Graz Flughafen
for year in range(1990, 2021):
    base_uuid = "8d15d14d-2eba-4d36-b2ba-aaaaaaaa"
    store.import_with_id("../datasets/ZAMG/Steiermark/Graz_Flughafen/ZAMG_Jahrbuch_%s.csv" % year,
                         "%s%s" % (base_uuid, year))

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=PORT, use_reloader=False, debug=True)
