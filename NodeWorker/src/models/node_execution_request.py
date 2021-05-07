from src.models.message import Message


class NodeExecutionRequest(Message):

    def __init__(self, deserialized: dict):
        super().__init__()
        self.pipeline_id: str = deserialized['PipelineId']
        self.execution_id: str = deserialized['ExecutionId']
        self.node_id = deserialized['NodeId']
        self.operation_id: str = deserialized['OperationId']
        self.operation_name: str = deserialized['OperationName']
        self.result_key: str = deserialized['ResultKey']

    def get_pipeline_id(self) -> str:
        return self.pipeline_id

    def get_node_id(self) -> str:
        return self.node_id

    def get_execution_id(self) -> str:
        return self.execution_id

    def get_operation_id(self):
        return self.operation_id

    def get_operation_name(self):
        return self.operation_name

    def get_result_key(self):
        return self.result_key
