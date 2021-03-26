import datetime
import uuid

from src.models.simple_block_execution_request import SimpleBlockExecutionRequest
from src.models.simple_block_execution_response import SimpleBlockExecutionResponse
from src.services.dateset_service_client import DatasetServiceClient
from src.services.operation_service import OperationService


class BlockExecutionService:
    count: int

    def __init__(self, logging, dataset_client: DatasetServiceClient, operation_service: OperationService) -> None:
        self.count = 0
        self.logging = logging
        self.dataset_client = dataset_client
        self.operation_service = operation_service
        super().__init__()

    def handle_simple_request(self, request: SimpleBlockExecutionRequest) -> SimpleBlockExecutionResponse:
        self.count += 1
        self.logging.debug("Handling request %d" % self.count)

        response = SimpleBlockExecutionResponse()
        response.set_pipeline_id(request.pipeline_id)
        response.set_block_id(request.block_id)
        response.set_execution_id(request.execution_id)
        response.set_successful(True)
        response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

        if request.get_input_dataset_id() is not None:
            dataset = self.dataset_client.get_dataset_by_id(request.get_input_dataset_id())
        else:
            dataset = self.dataset_client.get_dataset_by_hash(request.get_input_dataset_hash())
        operation = request.get_operation_name()
        operation_config = self.preprocess_operation_config(request.get_operation_configuration())

        try:
            resulting_dataset = self.execute_simple_operation(
                dataset, operation, request.operation_id, operation_config)

            self.dataset_client.store_with_hash(request.get_result_key(), resulting_dataset)

            response.set_result_dataset_id(str(uuid.uuid4()))
            response.set_successful(True)
        except Exception as e:
            self.logging.warning("Failed to execute operation %s: %s" % (operation, str(e)))
            response.set_successful(False)
            response.set_error_description(str(e))

        response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))

        return response

    def preprocess_operation_config(self, config: dict) -> dict:
        for key in config:
            try:
                config[key] = int(config[key])
            except ValueError:
                try:
                    config[key] = float(config[key])
                except ValueError:
                    config[key] = config[key]
        return config

    def execute_simple_operation(self, df, operation: str, operation_id: str, operation_config: dict):
        self.logging.info("Executing operation %s (%s)" % (operation, operation_id))

        op = self.operation_service.get_operation_by_id(operation_id)
        op(df)

        if operation == 'select_columns':
            resulting_dataset = df[dict[0]]
        else:
            command = ("resulting_dataset = df.%s(**operation_config)" % operation)
            loc = {
                'df': df,
                'operation_config': operation_config
            }
            exec(command, globals(), loc)
            resulting_dataset = loc['resulting_dataset']
        print(resulting_dataset)

        return resulting_dataset
