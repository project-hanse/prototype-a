from src.models.block_execution_request import BlockExecutionRequest


class SimpleBlockExecutionRequest(BlockExecutionRequest):

    def __init__(self, deserialized: dict):
        super().__init__(deserialized)

        self.input_data_set_id: str = deserialized['InputDataSetId']
        self.producing_block_hash: str = deserialized['ProducingBlockHash']
        self.operation_name: str = deserialized['OperationName']
        self.operation_configuration: dict = deserialized['OperationConfiguration']
