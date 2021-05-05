from src.models.node_execution_request import NodeExecutionRequest


class NodeExecutionRequestSingleInput(NodeExecutionRequest):

    def __init__(self, deserialized: dict):
        super().__init__(deserialized)

        self.input_dataset_id: str = deserialized['InputDataSetId']
        self.input_dataset_hash: str = deserialized['InputDataSetHash']
        self.operation_id: str = deserialized['OperationId']
        self.operation_name: str = deserialized['OperationName']
        self.operation_configuration: dict = deserialized['OperationConfiguration']
        self.result_key: str = deserialized['ResultKey']

    def get_operation_id(self):
        return self.operation_id

    def get_operation_name(self):
        return self.operation_name

    def get_input_dataset_id(self):
        return self.input_dataset_id

    def get_input_dataset_hash(self):
        return self.input_dataset_hash

    def get_result_key(self):
        return self.result_key

    def get_operation_configuration(self):
        return self.operation_configuration
