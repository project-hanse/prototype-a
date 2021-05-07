import datetime
import logging

import pandas as pd

from src.models.node_execution_request import NodeExecutionRequest
from src.models.node_execution_request_single_input import NodeExecutionRequestSingleInput
from src.models.node_execution_response import NodeExecutionResponse
from src.models.node_execution_response_single_input import NodeExecutionResponseSingleInput
from src.services.dateset_service_client import DatasetServiceClient
from src.services.operation_service import OperationService


class NodeExecutionService:
    count: int

    def __init__(self, logger: logging,
                 dataset_client: DatasetServiceClient,
                 operation_service: OperationService) -> None:
        self.count = 0
        self.logger = logger
        self.dataset_client = dataset_client
        self.operation_service = operation_service
        super().__init__()

    def handle_single_input_request(self, request: NodeExecutionRequestSingleInput) -> NodeExecutionResponseSingleInput:
        self.count += 1
        self.logger.debug("Handling request for single input operation %d" % self.count)

        response = NodeExecutionResponseSingleInput()
        self.set_initial_response_values(request, response)

        df_id = request.get_input_dataset_id()
        df_hash = request.get_input_dataset_hash()

        # Loading dataset from dedicated service (using successful state to check if loading was successful)
        response.set_successful(True)
        dataset = self.load_dataset_from_service(df_hash, df_id, response)
        if not response.get_successful():
            # Dataset loading not successful -> returning error message
            return response

        operation = request.get_operation_name()
        operation_config = self.preprocess_operation_config(request.get_operation_configuration())

        # Executing operation
        try:
            resulting_dataset = self.execute_simple_operation(
                dataset, operation, request.operation_id, operation_config)

            self.dataset_client.store_with_hash(request.get_result_key(), resulting_dataset)

            response.set_dataset_producing_hash(request.get_result_key())
            response.set_successful(True)
        except Exception as e:
            self.logger.warning("Failed to execute operation %s: %s" % (operation, str(e)))
            response.set_successful(False)
            response.set_error_description(str(e))

        response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

        return response

    def handle_double_input_request(self, request):
        self.count += 1
        self.logger.debug("Handling request for single input operation %d" % self.count)

    def set_initial_response_values(self, request: NodeExecutionRequest, response: NodeExecutionResponse):
        self.logger.debug("Generating basic response message")
        response.set_pipeline_id(request.pipeline_id)
        response.set_node_id(request.node_id)
        response.set_execution_id(request.execution_id)
        response.set_successful(True)
        response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

    def execute_simple_operation(self, df: pd.DataFrame,
                                 operation_name: str,
                                 operation_id: str,
                                 operation_config: dict):
        self.logger.info("Executing operation %s (%s)" % (operation_name, operation_id))

        operation_callable = self.operation_service.get_simple_operation_by_id(operation_id)

        # TODO catch method signature mismatch exception
        resulting_dataset = operation_callable(self.logger, operation_name, operation_config, df)

        return resulting_dataset

    def load_dataset_from_service(self, df_hash: str, df_id: str, response: NodeExecutionResponse) -> pd.DataFrame:
        dataset = None
        try:
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
                    config[key] = config[key]
        return config
