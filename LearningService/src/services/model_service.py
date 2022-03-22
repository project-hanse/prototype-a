from mlflow.tracking import MlflowClient

from src.helper.log_helper import LogHelper
from src.services.trainer_registry import TrainerRegistry


class ModelService:
	def __init__(self, client: MlflowClient, model_registry: TrainerRegistry):
		self.model_registry = model_registry
		self.client = client
		self.logger = LogHelper.get_logger(__name__)

	def get_models(self):
		models = self.client.list_registered_models()
		self.logger.info("Found %d models" % len(models))
		return models

	def train_model(self, model_name: str):
		self.logger.info("Training model %s" % model_name)
		trainer = self.model_registry.get_trainer_by_model_name(model_name)
		feat, lab = trainer.get_data()
		pipeline = trainer.get_model_pipeline()
		# model = pipeline.fit(feat, lab)
