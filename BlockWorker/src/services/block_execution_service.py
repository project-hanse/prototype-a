import datetime
import random
import time
import uuid

from src.models.simple_block_execution_request import SimpleBlockExecutionRequest
from src.models.simple_block_execution_response import SimpleBlockExecutionResponse


class BlockExecutionService:
    count: int

    def __init__(self, logging) -> None:
        self.count = 0
        self.logging = logging
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

        rand = random.randint(1, 6)
        self.logging.info("Simulating %s for %i seconds..." % (request.get_operation_name(), rand))
        time.sleep(rand)

        response.set_stop_time(datetime.datetime.now(datetime.timezone.utc))
        response.set_result_dataset_id(str(uuid.uuid4()))

        return response
