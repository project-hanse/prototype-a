import datetime

from src.models.simple_block_execution_request import SimpleBlockExecutionRequest
from src.models.simple_block_execution_response import SimpleBlockExecutionResponse
from src.services.dateset_service_client import DatasetServiceClient


class NodeExecutionService:
    """
    A service that executes node operations.
    """

    def __init__(self, logging, dataset_client: DatasetServiceClient) -> None:
        self.count = 0
        self.logging = logging
        self.dataset_client = dataset_client
        super().__init__()

    def handle_simple_request(self, request: SimpleBlockExecutionRequest) -> SimpleBlockExecutionResponse:
        self.logging.debug("Handling node request %d" % self.count)

        response = SimpleBlockExecutionResponse()
        response.set_pipeline_id(request.pipeline_id)
        response.set_block_id(request.block_id)
        response.set_execution_id(request.execution_id)
        response.set_successful(True)
        response.set_start_time(datetime.datetime.now(datetime.timezone.utc))

        return response
