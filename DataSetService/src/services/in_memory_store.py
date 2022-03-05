import os
import time
from pathlib import Path
from typing import Optional

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

	def get_df_by_key(self, key) -> Optional[pd.DataFrame]:
		self.log.info("Loading dataset by key %s" % str(key))

		if self.store.keys().__contains__(key):
			return self.store.get(key)
		else:
			return None

	def store_by_key(self, key: str, data):
		data_type = type(data)
		self.log.info("Storing %s with key %s" % (str(data_type), key))
		self.store[key] = {
			'type': data_type,
			'date': time.gmtime(),
			'data': data,
			'metadata': None
		}

	def get_by_key(self, key: str, data_type: type = None):
		if data_type is not None:
			self.log.info("Loading dataset by key %s and verifying type %s" % (str(key), str(data_type)))
		else:
			self.log.info("Loading dataset by key %s" % str(key))

		if key in self.store:
			data_object = self.store[key]
			if data_type is not None:
				if data_object['type'] is not data_type:
					self.log.warn("Data type of key %s is not %s" % (str(key), str(data_type)))
					return None
			return data_object['data']
		else:
			self.log.info("Key %s does not exist" % str(key))
			return None

	def get_metadata_by_key(self, key):
		self.log.info("Loading metadata by key %s" % str(key))
		if key not in self.store:
			self.log.info("Key %s does not exist" % str(key))
			return None
		data_object = self.store[key]
		metadata = None
		if 'metadata' in data_object:
			metadata = data_object['metadata']
			if metadata is None:
				self.log.info("Dataset %s (%s) does not have metadata" % (str(key), str(data_object['type'])))
				return None
		return metadata

	def generate_metadata_by_key(self, key: str):
		self.log.info("Generating metadata for key %s" % str(key))
		if key not in self.store:
			self.log.info("Key %s does not exist" % str(key))
			return None
		data_object = self.store[key]
		if data_object['metadata'] is not None:
			self.log.info("Metadata for dataset %s already exists" % str(key))
			return None
		if data_object['type'] is pd.DataFrame:
			metadata = data_object['data'].describe()
		elif data_object['type'] is pd.Series:
			metadata = data_object['data'].describe()
		else:
			self.log.warn("Data type %s is not supported" % str(data_object['type']))
			return None
		self.store[key]['metadata'] = metadata

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
