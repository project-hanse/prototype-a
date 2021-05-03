from src.models.message import Message


class NodeExecutionRequest(Message):

    def __init__(self, deserialized: dict):
        super().__init__()
        self.pipeline_id: str = deserialized['PipelineId']
        self.execution_id: str = deserialized['ExecutionId']
        self.node_id = deserialized['NodeId']

    def get_pipeline_id(self) -> str:
        return self.pipeline_id

    def get_node_id(self) -> str:
        return self.node_id

    def get_execution_id(self) -> str:
        return self.execution_id
