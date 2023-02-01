import time

import mlflow
import numpy as np
from keras import Model, Sequential
from keras.callbacks import EarlyStopping
from keras.layers import Dropout, Dense
from spektral.data import Dataset, BatchLoader
from spektral.layers import GCNConv, GlobalSumPool
from spektral.transforms import GCNFilter, Degree

from src.datasets.graph_dataset import GraphDataset
from src.helper.log_helper import LogHelper
from src.models.model_base import ModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class Model4GraphNeuralNetwork(ModelBase):
	# static parameters
	batch_size = 32
	max_epochs = 16 * 16
	patience = 16
	train_test_split = 0.75
	test_val_split = 0.5

	min_nodes_in_graph = 2
	max_nodes_in_graph = 64 * 4

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__()
		self.dataset = None
		self.pipeline_client = pipeline_client
		self.dataset_client = dataset_client
		self.logger = LogHelper.get_logger(__name__)

	def load(self, cache: bool) -> None:
		try:
			self.dataset = GraphDataset(self.pipeline_client, self.dataset_client, reload=not cache)
		except Exception as e:
			self.logger.error("Failed to load dataset", e)
			raise e

	def get_model_definition(self, n_hidden, n_labels) -> Model:
		self.logger.debug("Creating model definition")
		model = Sequential([
			GCNConv(n_hidden),
			GlobalSumPool(),
			Dropout(0.25),
			Dense(n_labels, 'softmax')
		])
		return model

	def _dataset_preprocessing(self, dataset: Dataset) -> Dataset:
		self.logger.debug("Preprocessing dataset...")
		max_degree = dataset.map(lambda g: g.a.sum(-1).max(), reduce=max)
		# Add one-hot encoded graph degree
		dataset.apply(Degree(int(max_degree)))
		# pre-processing of adjacency matrix
		dataset.apply(GCNFilter())
		return dataset

	def train(self, model_name: str) -> dict:
		ret = {
			'modelName': model_name,
		}

		# Preprocessing
		dataset = self._dataset_preprocessing(self.dataset)

		# Train/test split
		np.random.shuffle(dataset)
		split = int(self.train_test_split * len(dataset))
		data_train, data_test = dataset[:split], dataset[split:]
		split = int(self.test_val_split * len(data_test))
		data_test, data_val = data_test[:split], data_test[split:]

		ret['trainSize'] = len(data_train)
		ret['testSize'] = len(data_test)
		ret['valSize'] = len(data_val)

		self.logger.debug("Training %s with train size: %i, test size: %i", model_name, len(data_train), len(data_test))

		# Data loaders
		loader_train = BatchLoader(data_train, batch_size=self.batch_size, epochs=self.max_epochs)
		loader_val = BatchLoader(data_val, batch_size=self.batch_size)
		loader_test = BatchLoader(data_test, batch_size=self.batch_size)

		expr_name = model_name + "-experiment"
		mlflow.set_experiment(expr_name)
		mlflow.sklearn.autolog(disable=False)

		with mlflow.start_run():
			try:
				model = self.get_model_definition(32, self.dataset.n_labels)
				model.compile(optimizer='adam', loss='categorical_crossentropy', weighted_metrics=['accuracy'])

				start_time = time.time()
				model.fit(loader_train,
									steps_per_epoch=loader_train.steps_per_epoch,
									validation_data=loader_val,
									validation_steps=loader_val.steps_per_epoch,
									epochs=self.max_epochs,
									callbacks=[EarlyStopping(patience=self.patience, restore_best_weights=True)])
				end_time = time.time()

				metrics_test = model.evaluate(loader_test.load(), steps=loader_test.steps_per_epoch)
				metrics_val = model.evaluate(loader_val.load(), steps=loader_val.steps_per_epoch)
				ret['loss'] = metrics_test[0]
				ret['accuracy'] = metrics_test[1]
				ret['accuracyTest'] = metrics_test[1]
				ret['accuracyVal'] = metrics_val[1]
				self.logger.debug("Model training (model: %s) finished with loss: %f, accuracy: %f",
													model_name, metrics_test[0], metrics_test[1])

				mlflow.log_metric("training_timestamp", int(round(time.time() * 1000)))
				mlflow.log_metric("training_time", end_time - start_time)
				mlflow.log_metric("loss", metrics_test[0])
				mlflow.log_metric("loss_test", metrics_test[0])
				mlflow.log_metric("loss_val", metrics_val[0])
				mlflow.log_metric("accuracy", metrics_test[1])
				mlflow.log_metric("accuracy_test", metrics_test[1])
				mlflow.log_metric("accuracy_val", metrics_val[1])
				mlflow.log_metric("train_size", len(data_train))
				mlflow.log_metric("test_size", len(data_test))
				mlflow.log_metric("training_time", end_time - start_time)
				ret["timeFittingModel"] = end_time - start_time

			except Exception as e:
				self.logger.error("Model training failed: %s" % e)
			finally:
				mlflow.log_param("model_name", model_name)
				mlflow.end_run()
				mlflow.sklearn.autolog(disable=True)
		return ret
