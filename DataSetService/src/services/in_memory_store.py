import logging
import os

import pandas as pd


class InMemoryStore:
    store: dict = None

    def __init__(self) -> None:
        super().__init__()
        self.store = dict()
        self.logger = logging.getLogger('InMemoryStore')
        handler = logging.StreamHandler()
        handler.setFormatter(logging.Formatter("%(asctime)s [%(levelname)s] %(message)s"))
        self.logger.addHandler(handler)
        self.logger.setLevel(logging.INFO)

    def import_with_id(self, file: str, dataframe_id: str):
        dirname = os.path.dirname(__file__)
        filename = os.path.join(dirname, '../', file)

        self.logger.info('Importing %s and storing with id %s' % (filename, str(dataframe_id)))

        df = pd.read_csv(filename)
        self.store[dataframe_id] = df

    def get_dataset_count(self):
        return len(self.store)

    def get_by_id(self, dataframe_id: str):
        self.logger.info("Loading dataset by id %s" % str(dataframe_id))

        if self.store.keys().__contains__(dataframe_id):
            return self.store.get(dataframe_id)
        else:
            return None

    def get_by_hash(self, producing_hash):
        self.logger.info("Loading dataset by hash %s" % str(producing_hash))

        if self.store.keys().__contains__(producing_hash):
            return self.store.get(producing_hash)
        else:
            return None

    def store_data_set(self, key: str, data):
        self.logger.info("Storing data with key %s of shape %s" % (key, data.shape))
        self.store[key] = data
