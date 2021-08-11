import os
from pathlib import Path

import chardet
import pandas as pd

from src.helper.log_helper import LogHelper


class InMemoryStore:
    store: dict = None

    def __init__(self) -> None:
        super().__init__()
        self.store = dict()
        self.log = LogHelper.get_logger('InMemoryStore')

    def import_with_id(self, file: str, dataframe_id: str):
        filename = str(Path(file).resolve())

        if not os.path.isfile(filename):
            self.log.error("File %s does not exist", filename)
            return

        self.log.info('Importing %s and storing with id %s' % (filename, str(dataframe_id)))
        if file.endswith(".csv"):
            df = pd.read_csv(filename,
                             sep=self.get_sep(filename),
                             encoding=self.get_file_encoding(filename),
                             skiprows=self.get_skiprows(filename))
        elif file.endswith(".xlsx"):
            df = pd.read_excel(filename)
        else:
            self.log.error("This file (%s) is not supported" % filename)
            return
        self.store[dataframe_id] = df

    def get_dataset_count(self):
        return len(self.store)

    def get_ids(self):
        return self.store.keys()

    def get_by_id(self, dataframe_id: str):
        self.log.info("Loading dataset by id %s" % str(dataframe_id))

        if self.store.keys().__contains__(dataframe_id):
            return self.store.get(dataframe_id)
        else:
            return None

    def get_by_hash(self, producing_hash):
        self.log.info("Loading dataset by hash %s" % str(producing_hash))

        if self.store.keys().__contains__(producing_hash):
            return self.store.get(producing_hash)
        else:
            return None

    def store_data_set(self, key: str, data: pd.DataFrame):
        self.log.info("Storing data with key %s of shape %s" % (key, data.shape))
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

    @staticmethod
    def get_skiprows(file_path: str):
        """
        Get an array of line numbers that should be skipped when importing a csv file.
        :param file_path:
        :return:
        """
        # TODO: Implement in a general way e.g.: pass 1 get maximum separator count (, or ;) per row, pass 2 build
        #  array with row index that does not have this count
        if "ZAMG_Jahrbuch" in file_path:
            return 4
        return 0
