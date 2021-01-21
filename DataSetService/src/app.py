from flask import Flask, render_template
from flask_bootstrap import Bootstrap
from flask_jsonpify import jsonpify
from flask_socketio import SocketIO

from services.in_memory_store import InMemoryStore

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


@app.route('/api/datasets/<dataset_id>')
def get_dataset_by_id(dataset_id: str):
    df = store.get_by_id(dataset_id)

    if df is None:
        return None

    df_list = df.values.tolist()
    return jsonpify(df_list)


store.import_with_id("../datasets/Melbourne_housing_FULL.csv", "00e61417-cada-46db-adf3-a5fc89a3b6ee")
store.import_with_id("../datasets/MELBOURNE_HOUSE_PRICES_LESS.csv", "0c2acbdb-544b-4efc-ae54-c2dcba988654")

if __name__ == '__main__':
    socketio.run(app, host='0.0.0.0', port=5000, use_reloader=False, debug=True)
