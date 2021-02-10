from src.models.message import Message


class BlockExecutionRequest(Message):

    def __init__(self, deserialized: dict):
        super().__init__()
        self.pipeline_id: str = deserialized['PipelineId']
        self.execution_id: str = deserialized['ExecutionId']
        self.block_id = deserialized['BlockId']

    def get_pipeline_id(self) -> str:
        return self.pipeline_id

    def get_block_id(self) -> str:
        return self.block_id

    def get_execution_id(self) -> str:
        return self.execution_id
