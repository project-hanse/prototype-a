import mlflow
from mlflow.protos.model_registry_pb2 import RegisteredModel
from mlflow.tracking import MlflowClient

from src.helper.log_helper import LogHelper
from src.services.model_registry import ModelRegistry


class ModelService:
	def __init__(self, mlflow_client: MlflowClient, model_registry: ModelRegistry):
		self._registered_models_cache = None
		self.model_registry = model_registry
		self.mlflow_client: MlflowClient = mlflow_client
		self.train_split = 0.2
		self.cv_folds = 3
		self._model_cache = {}
		self.logger = LogHelper.get_logger(__name__)

	def get_models(self) -> [RegisteredModel]:
		models = self.mlflow_client.search_registered_models()
		self.logger.info("Found %d models" % len(models))
		return list(models)

	def get_model_dtos(self):
		models = self.get_models()
		model_dtos = []
		for model in models:
			latest_version = model.latest_versions[len(model.latest_versions) - 1]
			model_dtos.append({
				"name": model.name,
				"description": model.description,
				"latestVersion": latest_version.version,
				"creationTimestamp": latest_version.creation_timestamp,
				"lastUpdatedTimestamp": latest_version.last_updated_timestamp,
				"status": latest_version.status
			})
		return model_dtos

	def get_model(self, model_name: str):
		if model_name in self._model_cache:
			self.logger.debug("Model %s found in cache" % model_name)
			return self._model_cache[model_name]
		self.logger.info("Loading model %s" % model_name)
		registered_models = self.mlflow_client.search_registered_models("name='%s'" % model_name)
		if len(registered_models) == 0:
			raise Exception("No model found with name %s" % model_name)
		max_version = None
		# find model with the latest version
		for registered_model in registered_models:
			for version in registered_model.latest_versions:
				try:
					if max_version is None or int(version.version) > max_version:
						max_version = int(version.version)
				except Exception as e:
					self.logger.warning("Could not parse version %s - %s" % (version.version, e))

		self.logger.info("Found model %s with version %s" % (model_name, max_version))
		self._model_cache[model_name] = mlflow.sklearn.load_model(model_uri=f"models:/{model_name}/{max_version}")
		return self._model_cache[model_name]

	def get_all_models(self) -> [tuple]:
		if self._registered_models_cache is None:
			self._registered_models_cache = self.mlflow_client.search_registered_models()
		models = []
		for registered_model in self._registered_models_cache:
			models.append((registered_model.name, self.get_model(registered_model.name)))
		return models

	def train_model(self, model_name: str, cache_data: bool = True) -> dict:
		self.logger.info("Training model %s" % model_name)
		model = self.model_registry.get_model_by_name(model_name)
		model.load(cache_data)
		ret = model.train(model_name)
		self._model_cache.clear()
		self._registered_models_cache = None
		return ret

	def predict(self, data, model_name: str = None):
		if model_name is not None:
			self.logger.info("Predicting model %s" % model_name)
			model = self.get_model(model_name)
			try:
				return list(model.predict(data))
			except Exception as e:
				self.logger.error("Model prediction failed: %s" % e)
		self.logger.info("Predicting on all available models")
		models = self.get_all_models()
		predictions = []
		for model_name, model in models:
			try:
				predictions.extend(model.predict(data))
			except Exception as e:
				self.logger.error("Model prediction failed for model %s: %s" % (str(model), e))
		return predictions

	def train_all_model(self):
		self.logger.info("Training all models")
		rets = []
		for name in self.model_registry.get_all_model_names():
			rets.append(self.train_model(name, cache_data=True))
		return rets
