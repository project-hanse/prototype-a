import datetime
import json
import logging

import pandas as pd

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
			data = self.load_datasets(request.inputs, response)
			operation_config = self.preprocess_operation_config(request.get_operation_configuration())

			if response.get_error_description() is not None:
				return response

			# TODO Change method signature for plotting to make this special treatment obsolete
			if request.get_output().dataset_type == DatasetType.StaticPlot:
				self.execute_plotting_operation(data[0], request.worker_operation_identifier, request.worker_operation_id,
																				operation_config, request.get_output())
				success = self.file_store_client.store_file(request.get_output())
				if not success:
					response.set_error_description('Failed to store plot')
				response.set_successful(success)
			else:
				result = self.execute_operation(request.worker_operation_identifier, request.worker_operation_id,
																				operation_config, data)
				self.store_dataset(request.output, result)
		except Exception as e:
			self.logger.info("Failed to execute operation %s: %s" % (request.worker_operation_identifier, str(e)))
			response.set_successful(False)
			response.set_error_description(str(e))

		response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

		return response

	def set_initial_response_values(self, request: OperationExecutionMessage, response: OperationExecutedMessage):
		self.logger.debug("Generating basic response message")
		response.set_pipeline_id(request.pipeline_id)
		response.set_operation_id(request.operation_id)
		response.set_worker_operation_id(request.worker_operation_id)
		response.set_worker_operation_identifier(request.worker_operation_identifier)
		response.set_execution_id(request.execution_id)
		response.set_successful(True)
		response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

	def execute_operation(self, operation_name: str, operation_id: str, operation_config: dict, data: []):
		self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))
		operation_callable = self.operation_service.get_operation_by_id(operation_id)
		try:
			return operation_callable(self.logger, operation_name, operation_config, data)
		except Exception as e:
			self.logger.warning(
				"Failed to call operation method, trying backwards compatible signatures\n Error: %s" % str(e))
			try:
				return operation_callable(self.logger, operation_name, operation_config, data[0])
			except Exception as e:
				self.logger.warning(
					"Failed to call operation method, trying backwards compatible signatures\n Error: %s" % str(e))
				try:
					return operation_callable(self.logger, operation_name, operation_config, data[0], data[1])
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
		# TODO: implement remaining dataset types
		else:
			logging.error("%s is not a supported type" % dataset.get_type())
			raise NotImplemented("%s is not a supported type" % dataset.get_type())
