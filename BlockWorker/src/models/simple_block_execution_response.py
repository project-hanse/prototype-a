from src.models.block_execution_response import BlockExecutionResponse


class SimpleBlockExecutionResponse(BlockExecutionResponse):
    result_dataset_id: str

    def __init__(self) -> None:
        super().__init__()

    def set_result_dataset_id(self, id: str) -> None:
        self.result_dataset_id = id

    def get_result_dataset_id(self) -> str:
        return self.result_dataset_id
