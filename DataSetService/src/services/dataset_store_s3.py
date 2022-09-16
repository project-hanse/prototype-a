import pickle

import boto3
import pandas as pd
from botocore.config import Config
from expiringdict import ExpiringDict

from src.constants.bucket_names import DATASET_BUCKET_NAME, METADATA_BUCKET_NAME
from src.constants.metadata_constants import *
from src.helper.log_helper import LogHelper
from src.helper.s3_helper import assert_bucket_exists
from src.helper.type_helper import get_str_from_type, get_metadata_key
from src.models.dataset import Dataset


class DatasetStoreS3:
	dataset_cache: ExpiringDict = None
	metadata_cache: ExpiringDict = None

	def __init__(self) -> None:
		super().__init__()
		self.s3_client = None
		self.s3_region = None
		self.log = LogHelper.get_logger('S3Store')

	def setup(self, s3_endpoint: str, s3_access_key_id: str, s3_access_key_secret: str, s3_region: str):
		self.log.info("Setting up S3 dataset store (s3_endpoint: %s)" % s3_endpoint)
		self.dataset_cache = ExpiringDict(max_len=500, max_age_seconds=60 * 60)
		self.metadata_cache = ExpiringDict(max_len=500, max_age_seconds=60 * 60)

		try:
			self.s3_region = s3_region
			self.s3_client = boto3.client(service_name='s3',
																		endpoint_url=s3_endpoint,
																		aws_access_key_id=s3_access_key_id,
																		aws_secret_access_key=s3_access_key_secret,
																		config=Config(signature_version='s3v4'),
																		region_name=s3_region)
			assert_bucket_exists(self.log, self.s3_client, self.s3_region, DATASET_BUCKET_NAME)
			assert_bucket_exists(self.log, self.s3_client, self.s3_region, METADATA_BUCKET_NAME)
		except Exception as e:
			self.log.error("Failed to setup connection to S3 service %s" % str(e))

	def get_dataset_count(self):
		"""
		Returns the number of datasets in the store
		"""
		count = 0
		paginator = self.s3_client.get_paginator('list_objects')
		for result in paginator.paginate(Bucket=DATASET_BUCKET_NAME, Delimiter='/'):
			count += len(result['Contents'] if 'Contents' in result else [])
		return count

	def _compute_and_store_metadata(self, key: str, data):
		self.log.info("Computing metadata for key %s..." % str(key))
		metadata_compact = self.compute_metadata_compact(key, data)
		metadata_full = self.compute_metadata_full(key, data)
		self.log.info("Storing compact metadata for key %s..." % str(key))
		self.store_metadata_by_key(key, metadata_compact, METADATA_VERSION_COMPACT)
		self.log.info("Storing full metadata for key %s..." % str(key))
		self.store_metadata_by_key(key, metadata_full, METADATA_VERSION_FULL)
		self.log.info("Computed and stored metadata for key %s" % str(key))

	def store_metadata_by_key(self, key: str, metadata: dict, version: str = METADATA_VERSION_COMPACT):
		self.log.debug("Storing metadata for key %s" % str(key))
		metadata_pickled = pickle.dumps(metadata)
		self.s3_client.put_object(Bucket=METADATA_BUCKET_NAME,
															Key=get_metadata_key(key, version),
															Body=metadata_pickled)
		self.metadata_cache[get_metadata_key(key, version)] = metadata

	def store_data_by_key(self, key: str, data):
		data_type = type(data)
		self.log.debug("Storing %s with key %s" % (get_str_from_type(data_type), key))
		# pickle data
		data_pickled = pickle.dumps(data)
		response = self.s3_client.put_object(Bucket=DATASET_BUCKET_NAME,
																				 Key=key,
																				 Body=data_pickled)
		if response['ResponseMetadata']['HTTPStatusCode'] == 200:
			self.dataset_cache[key] = data
			self.log.info("Successfully stored %s with key %s" % (get_str_from_type(data_type), key))
			self._compute_and_store_metadata(key, data)
			return True
		self.log.error("Failed to store %s with key %s" % (get_str_from_type(data_type), key))
		return False

	def get_by_key(self, key: str, data_type: type = None):
		if data_type is not None:
			self.log.info("Loading dataset by key %s and verifying type %s" % (str(key), str(data_type)))
		else:
			self.log.info("Loading dataset by key %s" % str(key))

		if key not in self.dataset_cache:
			self.log.debug("Dataset with key %s not found in cache - loading from S3..." % str(key))
			try:
				response = self.s3_client.get_object(Bucket=DATASET_BUCKET_NAME, Key=key)
			except Exception as e:
				self.log.error("Failed to load dataset with key %s from S3: %s" % (str(key), str(e)))
				return None
			if response is None:
				self.log.info("Dataset with key %s does not exist" % str(key))
				return None
			data_pickled = response['Body'].read()
			data = pickle.loads(data_pickled)
			self.dataset_cache[key] = data
			self.log.info("Dataset with key %s loaded from S3" % str(key))
			return data
		else:
			self.log.info("Dataset with key %s found in cache" % str(key))
			return self.dataset_cache.get(key)

	def delete_dataset(self, dataset: Dataset) -> bool:
		self.log.debug("Deleting dataset with key %s" % str(dataset.key))
		if dataset.key in self.dataset_cache:
			self.log.debug("Dataset with key %s found in cache - removing from cache" % str(dataset.key))
			del self.dataset_cache[dataset.key]
		try:
			self.s3_client.delete_object(Bucket=DATASET_BUCKET_NAME, Key=dataset.key)
		except Exception as e:
			self.log.error("Failed to delete dataset with key %s: %s" % (str(dataset.key), str(e)))
			return False
		self.log.info("Dataset with key %s deleted" % str(dataset.key))
		return True

	def delete_metadata(self, dataset: Dataset) -> bool:
		self.log.debug("Deleting metadata for dataset with key %s" % str(dataset.key))
		if get_metadata_key(dataset.key, METADATA_VERSION_FULL) in self.metadata_cache:
			self.log.debug("Metadata for dataset with key %s found in cache - removing from cache" % str(dataset.key))
			del self.metadata_cache[get_metadata_key(dataset.key, METADATA_VERSION_FULL)]
		if get_metadata_key(dataset.key, METADATA_VERSION_COMPACT) in self.metadata_cache:
			self.log.debug("Metadata for dataset with key %s found in cache - removing from cache" % str(dataset.key))
			del self.metadata_cache[get_metadata_key(dataset.key, METADATA_VERSION_COMPACT)]
		try:
			self.s3_client.delete_object(Bucket=METADATA_BUCKET_NAME,
																	 Key=get_metadata_key(dataset.key, METADATA_VERSION_COMPACT))
			self.s3_client.delete_object(Bucket=METADATA_BUCKET_NAME,
																	 Key=get_metadata_key(dataset.key, METADATA_VERSION_FULL))
		except Exception as e:
			self.log.error("Failed to delete metadata for dataset with key %s: %s" % (str(dataset.key), str(e)))
			return False
		self.log.info("Metadata for dataset with key %s deleted" % str(dataset.key))
		return True

	def get_metadata_by_key(self, key, version: str = METADATA_VERSION_COMPACT):
		self.log.debug("Loading metadata by key %s" % str(key))
		metadata_key = get_metadata_key(key, version)
		if metadata_key not in self.metadata_cache:
			self.log.debug("Metadata with key %s not found in cache - loading from S3..." % str(metadata_key))
			try:
				response = self.s3_client.get_object(Bucket=METADATA_BUCKET_NAME, Key=metadata_key)
			except Exception as e:
				self.log.info("Failed to load metadata with key %s: %s" % (str(metadata_key), str(e)))
				return None
			if response is None:
				self.log.info("Metadata with key %s does not exist" % str(metadata_key))
				return None
			metadata_pickled = response['Body'].read()
			metadata = pickle.loads(metadata_pickled)
			self.metadata_cache[metadata_key] = metadata
			self.log.info("Metadata with key %s loaded from S3" % str(metadata_key))
			return metadata
		else:
			self.log.info("Metadata with key %s found in cache" % str(metadata_key))
			return self.metadata_cache.get(metadata_key)

	def extend_metadata_by_key(self, key: str, metadata: dict, version: str = METADATA_VERSION_COMPACT) -> bool:
		self.log.info("Storing metadata for key %s" % str(key))
		existing_metadata = self.get_metadata_by_key(key, version)
		if existing_metadata is None:
			self.log.info("No metadata found for this key (%s)" % str(key))
			return False
		existing_metadata.update(metadata)
		self.store_metadata_by_key(key, existing_metadata, version)
		# also update the full version
		if version == METADATA_VERSION_COMPACT:
			existing_metadata = self.get_metadata_by_key(key, METADATA_VERSION_FULL)
			existing_metadata.update(metadata)
			self.store_metadata_by_key(key, existing_metadata, METADATA_VERSION_FULL)
		return True

	def compute_metadata_full(self, col_key: str, data):
		self.log.debug("Computing full metadata for dataset '%s'" % str(col_key))
		metadata = self.compute_metadata_compact(col_key, data)
		if type(data) is pd.DataFrame:
			d = data.describe().to_dict()
			for col_key, value in d.items():
				for k, v in value.items():
					metadata['desc_' + str(col_key) + '_' + str(k)] = v
		elif type(data) is pd.Series:
			d = data.describe().to_dict()
			for key, value in d.items():
				metadata['desc_' + str(key)] = value
		return metadata

	def compute_metadata_compact(self, key: str, data) -> dict:
		self.log.debug("Computing compact metadata for dataset '%s'" % str(key))
		metadata = {
			'type': get_str_from_type(type(data))
		}
		if type(data) is pd.DataFrame:
			metadata['shape_rows'] = data.shape[0]
			metadata['shape_cols'] = data.shape[1]
			metadata['datatypes'] = self.get_datatype_list(data)
			metadata['columns'] = data.columns.values.tolist()
		elif type(data) is pd.Series:
			metadata['shape_rows'] = data.shape[0]
			metadata['shape_cols'] = 1
			metadata['datatype'] = str(data.dtype)
		return metadata

	@staticmethod
	def get_datatype_list(df: pd.DataFrame) -> [str]:
		dt_list = []
		df.dtypes.apply(lambda x: dt_list.append(str(x)))
		return dt_list
