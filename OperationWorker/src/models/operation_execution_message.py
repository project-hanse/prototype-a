from src.models.dataset import Dataset
from src.models.message import Message


class OperationExecutionMessage(Message):

	def __init__(self, deserialized: dict):
		super().__init__()
		self._pipeline_id: str = deserialized['PipelineId']
		self._execution_id: str = deserialized['ExecutionId']
		self._operation_id = deserialized['OperationId']
		self._worker_operation_id = deserialized['WorkerOperationId']
		self._worker_operation_identifier: str = deserialized['WorkerOperationIdentifier']
		self._operation_configuration: dict = deserialized['OperationConfiguration']
		self._inputs: [Dataset] = Dataset.from_array(deserialized['Inputs'])
		self._outputs: [Dataset] = Dataset.from_array(deserialized['Outputs'])

	def get_pipeline_id(self) -> str:
		return self._pipeline_id

	def get_operation_id(self) -> str:
		return self._operation_id

	def get_worker_operation_id(self) -> str:
		return self._worker_operation_id

	def get_worker_operation_identifier(self):
		return self._worker_operation_identifier

	def get_execution_id(self) -> str:
		return self._execution_id

	def get_operation_configuration(self) -> dict:
		return self._operation_configuration

	def get_inputs(self) -> [Dataset]:
		return self._inputs

	def get_outputs(self) -> [Dataset]:
		return self._outputs
