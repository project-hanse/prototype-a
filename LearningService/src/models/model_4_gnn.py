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

		# Model definition
		model = self.get_model_definition(32, self.dataset.n_labels)
		model.compile(optimizer='adam', loss='categorical_crossentropy', weighted_metrics=['accuracy'])

		model.fit(loader_train,
							steps_per_epoch=loader_train.steps_per_epoch,
							validation_data=loader_val,
							validation_steps=loader_val.steps_per_epoch,
							epochs=self.max_epochs,
							callbacks=[EarlyStopping(patience=self.patience, restore_best_weights=True)])

		metrics = model.evaluate(loader_test.load(), steps=loader_test.steps_per_epoch)
		ret['loss'] = metrics[0]
		ret['accuracy'] = metrics[1]
		return ret
