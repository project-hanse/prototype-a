import datetime

from src.models.message import Message


class OperationExecutedMessage(Message):
	pipeline_id: str
	execution_id: str
	operation_id: str
	worker_operation_id: str
	worker_operation_identifier: str
	successful: bool
	start_time: datetime
	stop_time: datetime
	error_description: str = None

	def __init__(self):
		super().__init__()

		self.start_time = datetime.datetime.now(datetime.timezone.utc)

	def set_pipeline_id(self, pipeline_id: str):
		self.pipeline_id = pipeline_id

	def set_operation_id(self, operation_id: str):
		self.operation_id = operation_id

	def set_worker_operation_id(self, worker_operation_id: str):
		self.worker_operation_id = worker_operation_id

	def set_worker_operation_identifier(self, worker_operation_id: str):
		self.worker_operation_identifier = worker_operation_id

	def set_execution_id(self, execution_id: str):
		self.execution_id = execution_id

	def set_successful(self, successful: bool):
		self.successful = successful

	def get_successful(self) -> bool:
		return self.successful

	def set_start_time(self, start_time: datetime):
		self.start_time = start_time

	def set_stop_time(self, stop_time: datetime):
		self.stop_time = stop_time

	def set_error_description(self, error_description):
		self.error_description = error_description

	def get_error_description(self) -> str:
		return self.error_description
