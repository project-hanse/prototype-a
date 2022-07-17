import random
import time

import mlflow
from mlflow.protos.model_registry_pb2 import RegisteredModel
from mlflow.tracking import MlflowClient
from sklearn.model_selection import train_test_split, cross_val_score

from src.helper.log_helper import LogHelper
from src.helper.mlflow_helper import infer_signature_custom
from src.services.trainer_registry import TrainerRegistry


class ModelService:
	def __init__(self, mlflow_client: MlflowClient, model_registry: TrainerRegistry):
		self._registered_models_cache = None
		self.model_registry = model_registry
		self.mlflow_client = mlflow_client
		self.train_split = 0.2
		self.cv_folds = 3
		self._model_cache = {}
		self.logger = LogHelper.get_logger(__name__)

	def get_models(self) -> [RegisteredModel]:
		models = self.mlflow_client.list_registered_models()
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
			self._registered_models_cache = self.mlflow_client.list_registered_models()
		models = []
		for registered_model in self._registered_models_cache:
			models.append((registered_model.name, self.get_model(registered_model.name)))
		return models

	def train_model(self, model_name: str, cache_data: bool = True):
		self.logger.info("Training model %s" % model_name)
		trainer = self.model_registry.get_trainer_by_model_name(model_name)
		load_data_start = time.time()
		feat, lab = trainer.get_data(cache=cache_data)
		load_data_end = time.time()
		X_train, X_test, y_train, y_test = train_test_split(feat, lab, test_size=self.train_split)
		search_cv = trainer.get_model_pipeline()
		expr_name = model_name + "-experiment"
		mlflow.set_experiment(expr_name)
		mlflow.sklearn.autolog(disable=False)
		ret = {}
		with mlflow.start_run():
			try:
				self.logger.info("Training model %s" % model_name)
				mlflow.log_metric("load_data_time", load_data_end - load_data_start)
				start_time = time.time()
				search_cv.fit(X_train, y_train)
				end_time = time.time()
				model = search_cv.best_estimator_
				accuracy = model.score(X_test, y_test)
				cvs = cross_val_score(model, X_train, y_train, scoring='accuracy', cv=self.cv_folds, n_jobs=-1)
				self.logger.info("Trained model %s with test accuracy %f and cross-validation accuracy %f" % (
					model_name, accuracy, cvs.mean()))
				mlflow.log_metric("training_timestamp", int(round(time.time() * 1000)))
				mlflow.log_param("training_timestamp", int(round(time.time() * 1000)))
				mlflow.log_metric("training_time", end_time - start_time)
				mlflow.log_param("model_name", model_name)
				mlflow.log_param("best_params", search_cv.best_params_)
				mlflow.log_metric("accuracy", accuracy)
				mlflow.log_metric("cv_accuracy", cvs.mean())
				mlflow.log_metric("cv_accuracy_std", cvs.std())
				mlflow.log_metric("cv_min", cvs.min())
				mlflow.log_metric("cv_max", cvs.max())
				mlflow.log_metric("train_size", len(X_train))
				mlflow.log_metric("test_size", len(X_test))
				ret["modelName"] = model_name
				ret["accuracy"] = accuracy
				ret["cvAccuracy"] = cvs.mean()
				ret["trainSize"] = len(X_train)
				ret["testSize"] = len(X_test)
				ret["timeDataLoading"] = load_data_end - load_data_start
				ret["timeFittingModel"] = end_time - start_time
				if len(X_test) > 10:
					model_input = random.sample(X_test, 10)
					mode_output = model.predict(model_input)
					signature = infer_signature_custom(model_input, mode_output)
				else:
					signature = None
				mlflow.sklearn.log_model(
					sk_model=model,
					artifact_path=mlflow.get_artifact_uri().replace('s3://', ''),
					registered_model_name=model_name,
					signature=signature
				)
			except Exception as e:
				self.logger.error("Model training failed: %s" % e)
			finally:
				mlflow.log_param("model_name", model_name)
				mlflow.log_param("cross_validation_folds", self.cv_folds)
				mlflow.end_run()
				mlflow.sklearn.autolog(disable=True)
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
		for name in self.model_registry.get_all_trainer_names():
			rets.append(self.train_model(name, cache_data=True))
		return rets
