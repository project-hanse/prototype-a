import random
import time
from abc import abstractmethod

import mlflow
from sklearn.model_selection import cross_val_score, train_test_split
from sklearn.model_selection._search import BaseSearchCV

from src.helper.log_helper import LogHelper
from src.helper.mlflow_helper import infer_signature_custom
from src.models.model_base import ModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class SkLearnModelBase(ModelBase):
	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__()
		self.train_split = 0.2
		self.cv_folds = 3
		self.load_duration = None
		self.lab = None
		self.feat = None
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)

	@abstractmethod
	def get_model_pipeline(self) -> BaseSearchCV:
		raise NotImplementedError()

	@abstractmethod
	def get_data(self, cache=True) -> (list, list):
		raise NotImplementedError()

	def load(self, cache: bool) -> None:
		self.logger.debug("Loading data for '%s'", __name__)
		load_data_start = time.time()
		self.feat, self.lab = self.get_data(cache=cache)
		load_data_end = time.time()
		self.load_duration = load_data_end - load_data_start
		self.logger.info("Loading data for '%s' took %f seconds", (__name__, self.load_duration))

	def train(self, model_name: str) -> dict:
		X_train, X_test, y_train, y_test = train_test_split(self.feat, self.lab, test_size=self.train_split)
		search_cv = self.get_model_pipeline()
		expr_name = model_name + "-experiment"
		mlflow.set_experiment(expr_name)
		mlflow.sklearn.autolog(disable=False)
		ret = {}
		with mlflow.start_run():
			try:
				self.logger.info("Training model %s" % model_name)
				mlflow.log_metric("load_data_time", self.load_duration)
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
				ret["timeDataLoading"] = self.load_duration
				ret["timeFittingModel"] = end_time - start_time
				if len(X_test) > 10:
					model_input = random.sample(X_test, 10)
					mode_output = model.predict(model_input)
					signature = infer_signature_custom(model_input, mode_output)
				else:
					signature = None
				mlflow.sklearn.log_model(
					sk_model=model,
					artifact_path="model",
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

	def _load_data(self, cache=True) -> (list, list):
		self.logger.debug("Loading data for %s", __name__)
		tuples = self.pipeline_client.get_pipeline_tuples(cache=cache)
		tuples_with_metadata = []
		loaded_input_metadata_cnt = 0
		total_input_cnt = 0
		i = 0
		for op_tuple in tuples:
			i += 1
			if i % 150 == 0:
				self.logger.info("Loaded metadata for %i/%i tuples..." % (i, len(tuples)))

			op_tuple['targetInputMetadata'] = []
			for op_input in op_tuple['targetInputs']:
				metadata = {}
				total_input_cnt += 1
				try:
					metadata['dataset_type'] = op_input['type']
					metadata_rmt = self.dataset_client.get_metadata(op_input['key'], dataset_type=op_input['type'], cache=cache)
					metadata.update(metadata_rmt)
					if 'type' in metadata:
						metadata['storage_type'] = metadata['type']
						del metadata['type']
					op_tuple['targetInputMetadata'].append(metadata)
					loaded_input_metadata_cnt += 1
				except Exception as e:
					self.logger.warning("Failed to load %s (%s)" % (op_input['key'], str(e)))
					op_tuple['targetInputMetadata'].append(metadata)
					continue
			tuples_with_metadata.append(op_tuple)
		self.logger.info("Got %d tuples with metadata for %d input datasets" % (len(tuples), loaded_input_metadata_cnt))
		feat, lab = self._tuples_preprocessing(tuples_with_metadata)
		return feat, lab

	# Preprocess tuples
	def _flatten_dict(self, d, parent_key='', sep='_'):
		items = []
		for k, v in d.items():
			new_key = parent_key + sep + k if parent_key else k
			if isinstance(v, dict):
				items.extend(self._flatten_dict(v, new_key, sep=sep).items())
			elif isinstance(v, list):
				# Ignore lists for now
				pass
			else:
				items.append((new_key, v))
		return dict(items)

	def _tuples_preprocessing(self, data: []) -> ([{}], [{}]):
		feat = []
		lab = []
		for element in data:
			if "targetOperationIdentifier" not in element or "targetInputMetadata" not in element:
				self.logger.warning("Missing essential keys in element: %s", element)
				continue

			lab.append(element["targetOperationIdentifier"])
			input_dataset_count = 0
			feat_vec = {}
			for input_meta in element['targetInputMetadata']:
				for key, val in self._flatten_dict(input_meta, parent_key='input_' + str(input_dataset_count), sep='_').items():
					feat_vec[key] = val
				input_dataset_count += 1
			feat_vec["feat_pred_id"] = element['predecessorOperationIdentifier']
			feat_vec['feat_pred_input_count'] = len(element['targetInputs'])
			if feat_vec is {}:
				self.logger.warning("Empty feature vector for element: %s", element)
			feat.append(feat_vec)

		return feat, lab
