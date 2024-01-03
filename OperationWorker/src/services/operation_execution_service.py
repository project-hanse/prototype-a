import datetime
import json
import logging
import os

import pandas as pd

from src.helper.operations_helper import OperationsHelper
from src.models.dataset import Dataset
from src.models.dataset_type import DatasetType
from src.models.operation_executed_message import OperationExecutedMessage
from src.models.operation_execution_message import OperationExecutionMessage
from src.services.dateset_service_client import DatasetServiceClient
from src.services.file_store_client import FileStoreClient
from src.services.operation_service import OperationService


class OperationExecutionService:
	count: int

	def __init__(self, logger: logging,
							 dataset_client: DatasetServiceClient,
							 file_store_client: FileStoreClient,
							 operation_service: OperationService) -> None:
		self.count = 0
		self.logger = logger
		self.dataset_client = dataset_client
		self.file_store_client = file_store_client
		self.operation_service = operation_service
		super().__init__()

	def handle_execution_request(self, request: OperationExecutionMessage) -> OperationExecutedMessage:
		self.count += 1
		self.logger.debug("Handling request for no input operation %d" % self.count)

		response = OperationExecutedMessage()
		self.set_initial_response_values(request, response)
		try:
			data = self.load_datasets(request.get_inputs(), response)
			operation_config = self.preprocess_operation_config(request.get_operation_configuration())
			self.set_missing_configurations(operation_config, request)

			if response.get_error_description() is not None:
				return response

			results = self.execute_operation(request._worker_operation_identifier,
																			 request._worker_operation_id,
																			 operation_config, data)

			if type(results) is not list:
				self.logger.warning("Fixing backwards compatibility: results is not a list")
				results = [results]

			store_start = datetime.datetime.now(datetime.timezone.utc)
			self.logger.info("Storing %d resulting dataset(s) (start_time: %s UTC, size: %.2f kB)..." % (
				len(results), str(store_start), len(str(results)) / 1024))
			for output, result in zip(request.get_outputs(), results):
				try:
					self.store_dataset(output, result)
				except Exception as e:
					self.logger.info("Failed to store dataset %s: %s" % (output.get_key(), str(e)))
					response.set_successful(False)
					response.set_error_description(str(e))
				finally:
					store_duration = datetime.datetime.now(datetime.timezone.utc) - store_start
					self.logger.info("Storing %d resulting dataset(s) took %s" % (len(results), str(store_duration)))

		except Exception as e:
			self.logger.info("Failed to execute operation %s: %s" % (request._worker_operation_identifier, str(e)))
			response.set_successful(False)
			response.set_error_description(str(e))

		response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

		return response

	def set_initial_response_values(self, request: OperationExecutionMessage, response: OperationExecutedMessage):
		self.logger.debug("Generating basic response message")
		response.set_pipeline_id(request._pipeline_id)
		response.set_operation_id(request._operation_id)
		response.set_worker_operation_id(request._worker_operation_id)
		response.set_worker_operation_identifier(request._worker_operation_identifier)
		response.set_execution_id(request._execution_id)
		response.set_successful(True)
		response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

	def execute_operation(self, operation_name: str, operation_id: str, operation_config: dict, data: []):
		self.logger.info("Executing operation %s (%s)..." % (operation_name, operation_id))
		operation_callable = self.operation_service.get_operation_by_id(operation_id)
		try:
			return operation_callable(self.logger, operation_name, operation_config, data)
		except Exception as e:
			self.logger.warning(
				"Failed to call operation method, trying backwards compatible signatures\n Error: %s" % str(e))
			try:
				return operation_callable(self.logger, operation_name, operation_config, data[0], data[1])
			except Exception as e:
				self.logger.warning(
					"Failed to call operation method, trying backwards compatible signatures\n Error: %s" % str(e))
				try:
					return operation_callable(self.logger, operation_name, operation_config, data[0])
				except Exception as e:
					self.logger.error("Failed to call operation method Error: %s" % str(e))
					raise e

	def execute_plotting_operation(self, df: pd.DataFrame, operation_name: str, operation_id: str,
																 operation_config: dict, output: Dataset) -> None:

		self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))

		operation_callable = self.operation_service.get_operation_by_id(operation_id)
		operation_callable(self.logger, operation_config, df, output)

	@staticmethod
	def preprocess_operation_config(config: dict) -> dict:
		for key in config:
			try:
				config[key] = int(config[key])
			except ValueError:
				try:
					config[key] = float(config[key])
				except ValueError:
					try:
						if config[key].strip().startswith("{") or config[key].strip().startswith("["):
							cleaned_str = config[key].replace("'", '"')
							parsed = json.loads(cleaned_str)
							config[key] = OperationExecutionService.preprocess_operation_config(parsed)
						elif config[key].strip().lower() == 'none':
							config[key] = None
						elif config[key].strip().lower() == 'false':
							config[key] = False
						elif config[key].strip().lower() == 'true':
							config[key] = True
						else:
							raise ValueError
					except ValueError:
						try:
							config[key] = json.loads(config[key])
						except Exception:
							config[key] = config[key]
			except TypeError:
				continue
		return config

	def load_datasets(self, datasets: [], response: OperationExecutedMessage) -> []:
		self.logger.debug("Loading %i datasets" % len(datasets))
		loaded_datasets = []
		for dataset in datasets:
			# TODO implement local caching mechanism
			try:
				loaded_datasets.append(self.load_dataset(dataset))
			except Exception as e:
				self.logger.warning("Failed to load dataset %s\nError: %s" % (dataset.key + '@' + dataset.store, str(e)))
				response.set_successful(False)
				response.set_error_description("Failed to load dataset of type %s" % dataset.get_type())
				response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))
		self.logger.info("Loaded %i datasets" % len(datasets))
		return loaded_datasets

	def load_dataset(self, dataset: Dataset):
		self.logger.debug("Loading dataset of type %s" % str(dataset.get_type()))
		if dataset.get_type() == DatasetType.File:
			file_content = self.file_store_client.get_object_content_as_str(dataset.store, dataset.key)
			if file_content is None:
				file_content = self.file_store_client.get_object_content_as_binary_stream(dataset.store, dataset.key)
			return file_content
		elif dataset.get_type() == DatasetType.PdDataFrame:
			return self.dataset_client.get_dataframe_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.PdSeries:
			return self.dataset_client.get_series_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.Prophet:
			return self.dataset_client.get_prophet_model_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.SklearnModel:
			return self.dataset_client.get_sklearn_model_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.Dict:
			return self.dataset_client.get_dict_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.DictVectorizer:
			return self.dataset_client.get_dict_vectorizer_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.NpArray:
			return self.dataset_client.get_numpy_array_by_key(dataset.get_key())
		elif dataset.get_type() == DatasetType.SklearnEncoder:
			return self.dataset_client.get_sklearn_encoder_by_key(dataset.get_key())
		# TODO: implement remaining dataset types
		else:
			raise NotImplemented("%s is not a supported type" % dataset.get_type())

	def store_dataset(self, dataset: Dataset, data):
		self.logger.debug("Storing dataset of type %s" % str(dataset.get_type()))
		# TODO implement local caching mechanism
		if dataset.get_type() == DatasetType.PdDataFrame:
			self.dataset_client.store_dataframe_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.PdSeries:
			self.dataset_client.store_series_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.StaticPlot:
			self.file_store_client.store_file(dataset)
		elif dataset.get_type() == DatasetType.Prophet:
			self.dataset_client.store_prophet_model_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.File:
			self.file_store_client.store_file(dataset)
		elif dataset.get_type() == DatasetType.SklearnModel:
			self.dataset_client.store_sklearn_model(dataset, data)
		elif dataset.get_type() == DatasetType.Dict:
			self.dataset_client.store_dict_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.DictVectorizer:
			self.dataset_client.store_dict_vectorizer_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.NpArray:
			self.dataset_client.store_numpy_array_by_key(dataset.key, data)
		elif dataset.get_type() == DatasetType.SklearnEncoder:
			self.dataset_client.store_sklearn_encoder(dataset, data)
		# TODO: implement remaining dataset types
		else:
			logging.error("%s is not a supported type" % dataset.get_type())
			raise NotImplemented("%s is not a supported type" % dataset.get_type())

	def set_missing_configurations(self, operation_config, request):
		self.logger.debug("Setting missing configurations")
		for output in request.get_outputs():
			if output.get_type() == DatasetType.StaticPlot:
				target_path = OperationsHelper.get_temporary_file_path(output)
				filename, file_extension = os.path.splitext(target_path)
				operation_config['output_file_extension'] = file_extension
				operation_config['output_file_name'] = filename
				operation_config['output_file_path'] = target_path
				if file_extension == '':
					self.logger.warning(
						"No file extension found for dataset '%s' store '%s'" % (output.get_key(), output.get_store()))
					raise ValueError(
						"No file extension found for dataset '%s' store '%s'" % (output.get_key(), output.get_store()))
