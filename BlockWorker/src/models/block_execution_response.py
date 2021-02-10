import datetime
import json

from src.models.message import Message


class BlockExecutionResponse(Message):
    pipeline_id: str
    execution_id: str
    block_id: str
    successful: bool
    start_time: datetime
    stop_time: datetime

    def __init__(self):
        super().__init__()

        self.start_time = datetime.datetime.now(datetime.timezone.utc)

    def set_pipeline_id(self, pipeline_id: str):
        self.pipeline_id = pipeline_id

    def set_block_id(self, block_id: str):
        self.block_id = block_id

    def set_execution_id(self, execution_id: str):
        self.execution_id = execution_id

    def set_successful(self, successful: bool):
        self.successful = successful

    def set_start_time(self, start_time: datetime):
        self.start_time = start_time

    def set_stop_time(self, stop_time: datetime):
        self.stop_time = stop_time
