class BlockExecutionRequest:

    def __init__(self, deserialized: dict):
        super().__init__()
        self.pipeline_id: str = deserialized['PipelineId']
        self.execution_id: str = deserialized['ExecutionId']
        self.block_id = deserialized['BlockId']
