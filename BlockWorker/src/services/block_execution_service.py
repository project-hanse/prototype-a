import datetime
import uuid

from src.models.simple_block_execution_request import SimpleBlockExecutionRequest
from src.models.simple_block_execution_response import SimpleBlockExecutionResponse
from src.services.dateset_service_client import DatasetServiceClient


class BlockExecutionService:
    count: int

    def __init__(self, logging, dataset_client: DatasetServiceClient) -> None:
        self.count = 0
        self.logging = logging
        self.dataset_client = dataset_client
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

        if operation == 'dropna':
            resulting_dataset = dataset.dropna(
                **self.preprocess_operation_config(request.get_operation_configuration()))
            print(resulting_dataset)
        else:
            command = 'dataset.' + request.get_operation_name() + '(**self.preprocess_operation_config(request.get_operation_configuration()))'
            resulting_dataset = exec(command)

        self.dataset_client.store_with_hash(request.get_result_key(), resulting_dataset)

        response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))
        response.set_result_dataset_id(str(uuid.uuid4()))

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
