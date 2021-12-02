from src.models.dataset import Dataset
from src.models.message import Message


class OperationExecutionMessage(Message):

	def __init__(self, deserialized: dict):
		super().__init__()
		self.pipeline_id: str = deserialized['PipelineId']
		self.execution_id: str = deserialized['ExecutionId']
		self.operation_id = deserialized['OperationId']
		self.worker_operation_id = deserialized['WorkerOperationId']
		self.worker_operation_identifier: str = deserialized['WorkerOperationIdentifier']
		self.operation_configuration: dict = deserialized['OperationConfiguration']
		self.inputs: [] = Dataset.from_array(deserialized['Inputs'])
		self.output: Dataset = Dataset(deserialized['Output'])

	def get_pipeline_id(self) -> str:
		return self.pipeline_id

	def get_operation_id(self) -> str:
		return self.operation_id

	def get_worker_operation_id(self) -> str:
		return self.worker_operation_id

	def get_worker_operation_identifier(self):
		return self.worker_operation_identifier

	def get_execution_id(self) -> str:
		return self.execution_id

	def get_operation_configuration(self) -> dict:
		return self.operation_configuration

	def get_inputs(self) -> []:
		return self.inputs

	def get_output(self) -> Dataset:
		return self.output
