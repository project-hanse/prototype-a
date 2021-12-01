from src.models.node_execution_response import NodeExecutionResponse


class NodeExecutionResponseFileInput(NodeExecutionResponse):
    dataset_producing_hash: str

    def __init__(self) -> None:
        super().__init__()

    def set_dataset_producing_hash(self, producing_hash: str) -> None:
        self.dataset_producing_hash = producing_hash

    def get_result_dataset_id(self) -> str:
        return self.dataset_producing_hash
