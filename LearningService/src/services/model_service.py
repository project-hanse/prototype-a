from mlflow.tracking import MlflowClient

from src.helper.log_helper import LogHelper


class ModelService:
	def __init__(self, client: MlflowClient):
		self.client = client
		self.logger = LogHelper.get_logger(__name__)

	def get_models(self):
		models = self.client.list_registered_models()
		self.logger.info("Found %d models" % len(models))
		return models
