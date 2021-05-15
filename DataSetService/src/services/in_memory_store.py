import logging
import os
from pathlib import Path

import pandas as pd
import chardet


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
        filename = str(Path(file).resolve())

        if not os.path.isfile(filename):
            self.logger.error("File %s does not exist", filename)
            return

        self.logger.info('Importing %s and storing with id %s' % (filename, str(dataframe_id)))
        if file.endswith(".csv"):
            df = pd.read_csv(filename, sep=self.get_sep(filename), encoding=self.get_file_encoding(filename))
        elif file.endswith(".xlsx"):
            df = pd.read_excel(filename)
        else:
            self.logger.error("This file (%s) is not supported" % filename)
            return
        self.store[dataframe_id] = df

    def get_dataset_count(self):
        return len(self.store)

    def get_ids(self):
        return self.store.keys()

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

    def store_data_set(self, key: str, data: pd.DataFrame):
        self.logger.info("Storing data with key %s of shape %s" % (key, data.shape))
        self.store[key] = data

    @staticmethod
    def get_sep(file_path: str):
        """
        Checks if a given file (assuming it's a csv file) is separated by , or ;
        """
        # TODO: this should be made much more efficient but works for now
        df_comma = pd.read_csv(file_path, nrows=1, sep=",")
        df_semi = pd.read_csv(file_path, nrows=1, sep=";")
        if df_comma.shape[1] > df_semi.shape[1]:
            return ','
        else:
            return ';'

    @staticmethod
    def get_file_encoding(file_path: str):
        """
        Get the encoding of a file using chardet package
        :param file_path:
        """
        with open(file_path, 'rb') as f:
            result = chardet.detect(f.read())
            return result['encoding']
