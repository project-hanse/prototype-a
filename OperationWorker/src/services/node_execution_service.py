import datetime
import json
import logging

import pandas as pd

from src.models.operation_executed_message import OperationExecutedMessage
from src.models.operation_execution_message import OperationExecutionMessage
from src.services.dateset_service_client import DatasetServiceClient
from src.services.file_store_client import FileStoreClient
from src.services.operation_service import OperationService


class NodeExecutionService:
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

	def handle_file_input_request(self, request: OperationExecutionMessage) -> OperationExecutedMessage:
		self.count += 1
		self.logger.debug("Handling request for no input operation %d" % self.count)

		response = OperationExecutedMessage()
		self.set_initial_response_values(request, response)

		# TODO: Load directly with pandas (pd.read_csv(s3://...))
		# s3_path = 's3://localhost:4566/%s/%s' % (request.get_input_object_bucket(), request.get_input_object_key())

		operation_config = self.preprocess_operation_config(request.get_operation_configuration())

		try:
			file_content_str = self.file_store_client.get_object_content(request.get_inputs()[0].store,
																																	 request.get_inputs()[0].key)
			if file_content_str is not None:
				resulting_dataset = self.execute_file_input_operation(request.operation_identifier,
																															request.operation_id,
																															operation_config,
																															file_content_str)
			else:
				file_content_bin = self.file_store_client.get_object_content_as_binary(request.get_inputs()[0].store,
																																							 request.get_inputs()[0].key)
				resulting_dataset = self.execute_file_input_operation(request.operation_identifier,
																															request.operation_id,
																															operation_config,
																															file_content_bin)

			self.dataset_client.store_with_hash(request.get_output().key, resulting_dataset)
			response.set_successful(True)
		except Exception as e:
			self.logger.info("Failed to execute operation %s: %s" % (request.operation_identifier, str(e)))
			response.set_successful(False)
			response.set_error_description(str(e))

		response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))
		return response

	def handle_single_input_request(self, request: OperationExecutionMessage) -> OperationExecutedMessage:
		self.count += 1
		self.logger.debug("Handling request for single input operation %d" % self.count)

		response = OperationExecutedMessage()
		self.set_initial_response_values(request, response)

		# Loading dataset from dedicated service
		dataset = self.load_dataset_from_service(request.get_inputs()[0].key,
																						 request.get_inputs()[0].key,
																						 response)
		if dataset is None:
			# Dataset loading not successful -> returning error message
			return response

		operation = request.get_operation_identifier()
		operation_config = self.preprocess_operation_config(request.get_operation_configuration())

		# Executing operation
		try:
			resulting_dataset = self.execute_single_input_operation(dataset,
																															operation,
																															request.operation_identifier,
																															operation_config)

			self.dataset_client.store_with_hash(request.get_output().key, resulting_dataset)
			response.set_successful(True)
		except Exception as e:
			self.logger.info("Failed to execute operation %s: %s" % (operation, str(e)))
			response.set_successful(False)
			response.set_error_description(str(e))

		response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

		return response

	def handle_double_input_request(self, request: OperationExecutionMessage) -> OperationExecutedMessage:
		self.count += 1
		self.logger.debug("Handling request for double input operation %d" % self.count)
		response = OperationExecutedMessage()
		self.set_initial_response_values(request, response)

		dataset_one = self.load_dataset_from_service(request.get_inputs()[0].key,
																								 request.get_inputs()[0].key,
																								 response)
		if dataset_one is None:
			# Dataset loading not successful -> returning error message
			return response

		dataset_two = self.load_dataset_from_service(request.get_inputs()[1].key,
																								 request.get_inputs()[1].key,
																								 response)
		if dataset_two is None:
			# Dataset loading not successful -> returning error message
			return response

		operation = request.get_operation_identifier()
		operation_config = self.preprocess_operation_config(request.get_operation_configuration())

		# Executing operation
		try:
			resulting_dataset = self.execute_double_input_operation(dataset_one,
																															dataset_two,
																															operation,
																															request.operation_identifier,
																															operation_config)

			self.dataset_client.store_with_hash(request.get_output().key, resulting_dataset)

			response.set_successful(True)
		except Exception as e:
			self.logger.warning("Failed to execute operation %s: %s" % (operation, str(e)))
			response.set_successful(False)
			response.set_error_description(str(e))

		response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

		return response

	def set_initial_response_values(self, request: OperationExecutionMessage, response: OperationExecutedMessage):
		self.logger.debug("Generating basic response message")
		response.set_pipeline_id(request.pipeline_id)
		response.set_operation_id(request.operation_id)
		response.set_execution_id(request.execution_id)
		response.set_successful(True)
		response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

	# TODO: Merge execute_*_input_operation methods
	def execute_file_input_operation(self, operation_name: str,
																	 operation_id: str,
																	 operation_config: dict,
																	 file_content):
		self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))
		operation_callable = self.operation_service.get_file_operation_by_id(operation_id)

		# TODO catch method signature mismatch exception
		resulting_dataset = operation_callable(self.logger, operation_name, operation_config, file_content)

		return resulting_dataset

	def execute_single_input_operation(self, df: pd.DataFrame,
																		 operation_name: str,
																		 operation_id: str,
																		 operation_config: dict):
		self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))

		operation_callable = self.operation_service.get_simple_operation_by_id(operation_id)

		# TODO catch method signature mismatch exception
		resulting_dataset = operation_callable(self.logger, operation_name, operation_config, df)

		return resulting_dataset

	def execute_double_input_operation(self,
																		 df_one: pd.DataFrame,
																		 df_two: pd.DataFrame,
																		 operation_name: str,
																		 operation_id: str,
																		 operation_config: dict):
		self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))

		operation_callable = self.operation_service.get_simple_operation_by_id(operation_id)

		# TODO catch method signature mismatch exception
		resulting_dataset = operation_callable(self.logger, operation_name, operation_config, df_one, df_two)

		return resulting_dataset

	def load_dataset_from_service(self, df_id: str, df_hash: str, response: OperationExecutedMessage) -> pd.DataFrame:
		dataset = None
		try:
			# TODO: id is obsolete
			if df_id is not None:
				dataset = self.dataset_client.get_dataset_by_id(df_id)
			else:
				dataset = self.dataset_client.get_dataset_by_hash(df_hash)
		except Exception as e:
			self.logger.warning("Failed to load dataset: %s" % str(e))
			response.set_successful(False)
			response.set_error_description(str(e))
			response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))
		return dataset

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
							config[key] = NodeExecutionService.preprocess_operation_config(parsed)
						elif config[key].strip() == 'None':
							config[key] = None
						else:
							raise ValueError
					except ValueError:
						try:
							config[key] = json.loads(config[key])
						except Exception:
							config[key] = config[key]
			except TypeError:
				return config
		return config
