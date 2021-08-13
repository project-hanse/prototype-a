from src.models.node_execution_request import NodeExecutionRequest


class NodeExecutionRequestFileInput(NodeExecutionRequest):

    def __init__(self, deserialized: dict):
        super().__init__(deserialized)

        self.input_object_key: str = deserialized['InputObjectKey']
        self.input_object_bucket: str = deserialized['InputObjectBucket']
        self.operation_configuration: dict = deserialized['OperationConfiguration']

    def get_input_object_key(self):
        return self.input_object_key

    def get_input_object_bucket(self):
        return self.input_object_bucket

    def get_operation_configuration(self):
        return self.operation_configuration
