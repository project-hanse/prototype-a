from mlflow.tracking import MlflowClient
from sklearn.model_selection import train_test_split

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

	def train_model(self, model_name: str, cache_data: bool = True):
		self.logger.info("Training model %s" % model_name)
		trainer = self.model_registry.get_trainer_by_model_name(model_name)
		feat, lab = trainer.get_data(cache=cache_data)
		X_train, X_test, y_train, y_test = train_test_split(feat, lab, test_size=0.2)
		pipeline = trainer.get_model_pipeline()
		try:
			self.logger.info("Training model %s" % model_name)
			model = pipeline.fit(X_train, y_train)
			score = model.score(X_test, y_test)
			self.logger.info("Trained model %s with score %f.2" % (model_name, score))
			return pipeline
		except Exception as e:
			self.logger.error("Model training failed: %s" % e)
			return None
