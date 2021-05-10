from src.models.node_execution_request import NodeExecutionRequest


class NodeExecutionRequestSingleInput(NodeExecutionRequest):

    def __init__(self, deserialized: dict):
        super().__init__(deserialized)

        self.input_dataset_id: str = deserialized['InputDataSetId']
        self.input_dataset_hash: str = deserialized['InputDataSetHash']
        self.operation_configuration: dict = deserialized['OperationConfiguration']

    def get_input_dataset_id(self):
        return self.input_dataset_id

    def get_input_dataset_hash(self):
        return self.input_dataset_hash

    def get_operation_configuration(self):
        return self.operation_configuration
