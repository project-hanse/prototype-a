import random

import mlflow
from mlflow.tracking import MlflowClient
from sklearn.model_selection import train_test_split, cross_val_score

from src.helper.log_helper import LogHelper
from src.helper.mlflow_helper import infer_signature_custom
from src.services.trainer_registry import TrainerRegistry


class ModelService:
	def __init__(self, mlflow_client: MlflowClient, model_registry: TrainerRegistry):
		self.model_registry = model_registry
		self.mlflow_client = mlflow_client
		self.train_split = 0.2
		self.cv_folds = 3
		self.logger = LogHelper.get_logger(__name__)

	def get_models(self):
		models = self.mlflow_client.list_registered_models()
		self.logger.info("Found %d models" % len(models))
		return models

	def train_model(self, model_name: str, cache_data: bool = True):
		self.logger.info("Training model %s" % model_name)
		trainer = self.model_registry.get_trainer_by_model_name(model_name)
		feat, lab = trainer.get_data(cache=cache_data)
		X_train, X_test, y_train, y_test = train_test_split(feat, lab, test_size=self.train_split)
		pipeline = trainer.get_model_pipeline()
		expr_name = model_name + "-experiment"
		mlflow.set_experiment(expr_name)
		mlflow.sklearn.autolog(disable=False)
		ret = {}
		with mlflow.start_run():
			try:
				self.logger.info("Training model %s" % model_name)
				model = pipeline.fit(X_train, y_train)
				accuracy = model.score(X_test, y_test)
				cvs = cross_val_score(model, X_train, y_train, scoring='accuracy', cv=self.cv_folds, n_jobs=-1)
				self.logger.info("Trained model %s with test accuracy %f and cross-validation accuracy %f" % (
					model_name, accuracy, cvs.mean()))
				mlflow.log_metric("accuracy", accuracy)
				mlflow.log_metric("cv_accuracy", cvs.mean())
				mlflow.log_metric("cv_accuracy_std", cvs.std())
				mlflow.log_metric("cv_min", cvs.min())
				mlflow.log_metric("cv_max", cvs.max())
				mlflow.log_metric("train_size", len(X_train))
				mlflow.log_metric("test_size", len(X_test))
				ret["accuracy"] = accuracy
				ret["cv_accuracy"] = cvs.mean()
				ret["train_size"] = len(X_train)
				ret["test_size"] = len(X_test)
				signature = infer_signature_custom(random.choice(X_test), model.predict(random.choice(X_test)))
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
		return ret
