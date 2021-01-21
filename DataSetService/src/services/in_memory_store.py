import os

import pandas as pd


class InMemoryStore:
    store: dict = None

    def __init__(self) -> None:
        super().__init__()
        self.store = dict()

    def import_with_id(self, file: str, dataframe_id: str):
        dirname = os.path.dirname(__file__)
        filename = os.path.join(dirname, '../', file)

        print('Importing %s and storing with id %s' % (filename, str(dataframe_id)))

        df = pd.read_csv(filename)
        self.store[dataframe_id] = df

    def get_dataset_count(self):
        return len(self.store)

    def get_by_id(self, dataframe_id: str):
        print("Loading dataset by id %s" % str(dataframe_id))

        if self.store.keys().__contains__(dataframe_id):
            return self.store.get(dataframe_id)
        else:
            return None
