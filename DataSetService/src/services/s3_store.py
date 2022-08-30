import time
from typing import Optional

import pandas as pd

from src.constants.metadata_constants import *
from src.helper.log_helper import LogHelper
from src.helper.type_helper import get_type_str


class S3Store:
	store: dict = None

	def __init__(self) -> None:
		super().__init__()
		self.store = dict()
		self.log = LogHelper.get_logger('S3Store')

	def get_dataset_count(self):
		return len(self.store)

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
			'key': key,
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

	def get_metadata_by_key(self, key, version: str = METADATA_VERSION_COMPACT):
		self.log.info("Loading metadata by key %s" % str(key))
		if key not in self.store:
			self.log.info("Key %s does not exist" % str(key))
			return None
		data_object = self.store[key]
		metadata = None
		self.assert_metadata_exists(key)
		if 'metadata' in data_object:
			metadata = data_object['metadata'][version]
			if metadata is None:
				self.log.info("Dataset %s (%s) does not have metadata" % (str(key), str(data_object['type'])))
				return None
		metadata['type'] = get_type_str(data_object['type'])
		return metadata

	def store_metadata_by_key(self, key: str, metadata, version: str = METADATA_VERSION_COMPACT) -> bool:
		self.log.info("Storing metadata for key %s" % str(key))
		if key not in self.store:
			self.log.info("Dataset %s does not exist" % str(key))
			return False
		self.assert_metadata_exists(key)
		self.store[key]['metadata'][version] = metadata
		return True

	def generate_metadata_by_key(self, key: str, versions: [str] = None):
		if versions is None:
			versions = [METADATA_VERSION_COMPACT, METADATA_VERSION_FULL]
		self.log.info("Generating metadata for key %s" % str(key))
		if key not in self.store:
			self.log.info("Key %s does not exist" % str(key))
			return None
		data_object = self.store[key]
		self.assert_metadata_exists(key)
		for version in versions:
			if version == METADATA_VERSION_FULL:
				self.log.info("Generating full metadata for key %s" % str(key))
				if data_object['type'] is pd.DataFrame:
					metadata = data_object['data'].describe()
				elif data_object['type'] is pd.Series:
					metadata = data_object['data'].describe()
				else:
					self.log.warn("Data type %s is not supported" % str(data_object['type']))
					return None
				self.store[key]['metadata'][version] = metadata
			elif version == METADATA_VERSION_COMPACT:
				self.log.info("Generating compact metadata for key %s" % str(key))
				metadata = {}
				if data_object['type'] is pd.DataFrame:
					metadata['shape'] = str(data_object['data'].shape)
					metadata['datatypes'] = self.get_datatype_list(data_object['data'])
					metadata['columns'] = data_object['data'].columns.values.tolist()
					self.store[key]['metadata'][version] = metadata
				elif data_object['type'] is pd.Series:
					metadata['shape'] = str(data_object['data'].shape)
					metadata['datatype'] = str(data_object['data'].dtype)
					self.store[key]['metadata'][version] = metadata
			else:
				self.log.warn("Version %s is not supported" % str(version))
				return None

	def assert_metadata_exists(self, key):
		if key not in self.store:
			self.log.warn("Key %s does not exist" % str(key))
			return
		if self.store[key]['metadata'] is None:
			self.store[key]['metadata'] = {
				METADATA_VERSION_COMPACT: {},
				METADATA_VERSION_FULL: {}
			}

	@staticmethod
	def get_datatype_list(df: pd.DataFrame) -> [str]:
		dt_list = []
		df.dtypes.apply(lambda x: dt_list.append(str(x)))
		return dt_list
