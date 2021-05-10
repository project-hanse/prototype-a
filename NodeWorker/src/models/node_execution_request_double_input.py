from src.models.node_execution_request import NodeExecutionRequest


class NodeExecutionRequestDoubleInput(NodeExecutionRequest):
    def __init__(self, deserialized: dict):
        super().__init__(deserialized)

        self.input_dataset_one_id: str = deserialized['InputDataSetOneId']
        self.input_dataset_one_hash: str = deserialized['InputDataSetOneHash']
        self.input_dataset_two_id: str = deserialized['InputDataSetTwoId']
        self.input_dataset_two_hash: str = deserialized['InputDataSetTwoHash']
        self.operation_configuration: dict = deserialized['OperationConfiguration']

    def get_input_dataset_one_id(self):
        return self.input_dataset_one_id

    def get_input_dataset_one_hash(self):
        return self.input_dataset_one_hash

    def get_input_dataset_two_id(self):
        return self.input_dataset_two_id

    def get_input_dataset_two_hash(self):
        return self.input_dataset_two_hash

    def get_operation_configuration(self):
        return self.operation_configuration
