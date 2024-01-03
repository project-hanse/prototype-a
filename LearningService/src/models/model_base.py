from abc import abstractmethod

from src.helper.log_helper import LogHelper


class ModelBase:

	def __init__(self):
		self.logger = LogHelper.get_logger(__name__)

	@abstractmethod
	def load(self, cache: bool) -> None:
		raise NotImplementedError()

	@abstractmethod
	def train(self, model_name: str) -> dict:
		raise NotImplementedError()
