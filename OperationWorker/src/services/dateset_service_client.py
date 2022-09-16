import base64
import json
import pickle

import numpy as np
import pandas as pd
import requests
from prophet import Prophet
from prophet.serialize import model_to_json, model_from_json
from sklearn.base import BaseEstimator
from sklearn.feature_extraction import DictVectorizer

from src.exceptions.NotFoundError import NotFoundError
from src.exceptions.NotStoredError import NotStoredError
from src.helper.type_helper import get_type_str
from src.models.dataset import Dataset


class DatasetServiceClient:
	host: str
	port: int

	def __init__(self, host: str, port: int, logging) -> None:
		self.host = host
		self.port = port
		self.logging = logging

	def get_dataframe_by_id(self, dataset_id: str) -> pd.DataFrame:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/dataframe/' + dataset_id
		self.logging.info('Loading dataframe from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No dataset with id found")
		return pd.read_json(response.text)

	def get_dataframe_by_key(self, key: str) -> pd.DataFrame:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/dataframe/key/' + key
		self.logging.info('Loading dataframe from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No dataset with key found")
		return pd.read_json(response.text)

	def store_dataframe_by_key(self, key: str, resulting_dataset: pd.DataFrame):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/dataframe/key/' + key
		self.logging.debug('Storing dataframe to %s' % address)
		response = requests.post(address, data=self.serialize_dataframe(resulting_dataset))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store dataframe: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store dataframe: (%i) %s' % (response.status_code, str(response.reason)))

	def get_series_by_key(self, key: str) -> pd.Series:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/series/key/' + key
		self.logging.info('Loading series from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No dataset with key found")
		return pd.read_json(response.text, typ='series')

	def store_series_by_key(self, key: str, data: pd.Series):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/series/key/' + key
		self.logging.info('Storing series to %s' % address)
		response = requests.post(address, data=self.serialize_series(data))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store series: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store series: (%i) %s' % (response.status_code, str(response.reason)))

	def store_sklearn_model(self, dataset: Dataset, model: BaseEstimator):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + dataset.key
		self.logging.info('Storing sklearn model to %s' % address)
		serialized = self.serialize_sklearn_model(model)
		response = requests.post(address, data=serialized)
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store sklearn model: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store sklearn model: (%i) %s' % (response.status_code, str(response.reason)))

		address = 'http://' + self.host + ':' + str(self.port) + '/api/metadata/key/' + dataset.key + '?version=full'
		self.logging.info('Storing sklearn model metadata to %s' % address)
		full_metadata = model.get_params()
		full_metadata['model_type'] = get_type_str(type(model))
		self.store_metadata(address, full_metadata)

		address = 'http://' + self.host + ':' + str(self.port) + '/api/metadata/key/' + dataset.key + '?version=compact'
		self.logging.info('Storing sklearn model metadata to %s' % address)
		compact_metadata = {'model_type': get_type_str(type(model))}
		self.store_metadata(address, compact_metadata)

	def store_sklearn_encoder(self, dataset: Dataset, encoder: BaseEstimator):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + dataset.key
		self.logging.info('Storing sklearn encoder to %s' % address)
		serialized = self.serialize_sklearn_encoder(encoder)
		response = requests.post(address, data=serialized)
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store sklearn encoder: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store sklearn encoder: (%i) %s' % (response.status_code, str(response.reason)))

		address = 'http://' + self.host + ':' + str(self.port) + '/api/metadata/key/' + dataset.key + '?version=full'
		self.logging.info('Storing sklearn encoder metadata to %s' % address)
		full_metadata = encoder.get_params()
		full_metadata['encoder_type'] = get_type_str(type(encoder))
		self.store_metadata(address, full_metadata)

		address = 'http://' + self.host + ':' + str(self.port) + '/api/metadata/key/' + dataset.key + '?version=compact'
		self.logging.info('Storing sklearn encoder metadata to %s' % address)
		compact_metadata = {'encoder_type': get_type_str(type(encoder))}
		self.store_metadata(address, compact_metadata)

	def store_metadata(self, address, metadata):
		metadata_serialized = json.dumps(metadata)
		response = requests.post(address, data=metadata_serialized)
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning(
				'Failed to store sklearn model metadata: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError(
				'Failed to store sklearn model metadata: (%i) %s' % (response.status_code, str(response.reason)))

	def get_sklearn_model_by_key(self, key: str) -> BaseEstimator:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading sklearn model from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No model with key found")
		return self.deserialize_sklearn_model(response.text)

	def get_sklearn_encoder_by_key(self, key: str) -> BaseEstimator:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading sklearn encoder from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No encoder with key found")
		return self.deserialize_sklearn_encoder(response.text)

	def get_prophet_model_by_key(self, key: str) -> pd.DataFrame:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading prophet model from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No model with key found")
		return model_from_json(response.text)

	def store_prophet_model_by_key(self, key: str, model: Prophet):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Storing prophet model to %s' % address)
		response = requests.post(address, data=self.serialize_prophet_model(model))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store prophet model: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store prophet model: (%i) %s' % (response.status_code, str(response.reason)))

	def store_dict_by_key(self, key: str, dict: dict):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Storing dict to %s' % address)
		response = requests.post(address, data=json.dumps(dict))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store dict: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store dict: (%i) %s' % (response.status_code, str(response.reason)))

	def get_dict_by_key(self, key: str) -> dict:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading dict from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No dict with key found")
		return json.loads(response.text)

	def store_dict_vectorizer_by_key(self, key: str, vectorizer: DictVectorizer):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Storing dict vectorizer to %s' % address)
		response = requests.post(address, data=self.serialize_sklearn_model(vectorizer))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store dict vectorizer: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store dict vectorizer: (%i) %s' % (response.status_code, str(response.reason)))

	def get_dict_vectorizer_by_key(self, key: str) -> DictVectorizer:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading dict vectorizer from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No dict vectorizer with key found")
		return self.deserialize_sklearn_model(response.text)

	def store_numpy_array_by_key(self, key: str, array: np.ndarray):
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Storing numpy array to %s' % address)
		response = requests.post(address, data=self.serialize_numpy_array(array))
		if response.status_code < 300:
			self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
		else:
			self.logging.warning('Failed to store numpy array: (%i) %s' % (response.status_code, str(response.text)))
			raise NotStoredError('Failed to store numpy array: (%i) %s' % (response.status_code, str(response.reason)))

	def get_numpy_array_by_key(self, key: str) -> np.ndarray:
		address = 'http://' + self.host + ':' + str(self.port) + '/api/string/key/' + key
		self.logging.info('Loading numpy array from %s' % address)
		response = requests.get(address)
		if response.status_code == 404:
			raise NotFoundError("No numpy array with key found")
		return self.deserialize_numpy_array(response.text)

	def serialize_dataframe(self, dataframe: pd.DataFrame) -> str:
		try:
			return dataframe.to_json(date_format='iso')
		except ValueError as e:
			self.logging.warning('Error during serializing %s, trying to set index' % str(e))
			dataframe.reset_index(inplace=True)
			return dataframe.to_json(date_format='iso')

	def serialize_series(self, series: pd.Series) -> str:
		try:
			return series.to_json(date_format='iso')
		except ValueError as e:
			self.logging.warning('Error during serializing %s, trying to set index' % str(e))
			series.reset_index(inplace=True)
			return series.to_json(date_format='iso')

	def serialize_prophet_model(self, model: Prophet) -> str:
		self.logging.debug('Serializing prophet model')
		return model_to_json(model)

	def serialize_sklearn_model(self, model) -> str:
		self.logging.debug('Serializing sklearn model')
		b = pickle.dumps(model)
		s = base64.b64encode(b).decode('utf-8')
		return s

	def deserialize_sklearn_model(self, text: str):
		self.logging.debug('Deserializing sklearn model')
		b = base64.b64decode(text)
		m = pickle.loads(b)
		return m

	def serialize_sklearn_encoder(self, encoder) -> str:
		self.logging.debug('Serializing sklearn encoder')
		b = pickle.dumps(encoder)
		s = base64.b64encode(b).decode('utf-8')
		return s

	def deserialize_sklearn_encoder(self, text: str):
		self.logging.debug('Deserializing sklearn encoder')
		b = base64.b64decode(text)
		m = pickle.loads(b)
		return m

	def serialize_numpy_array(self, array: np.ndarray) -> str:
		self.logging.debug('Serializing numpy array')
		b = pickle.dumps(array)
		s = base64.b64encode(b).decode('utf-8')
		return s

	def deserialize_numpy_array(self, text: str) -> np.ndarray:
		self.logging.debug('Deserializing numpy array')
		b = base64.b64decode(text)
		m = pickle.loads(b)
		return m
