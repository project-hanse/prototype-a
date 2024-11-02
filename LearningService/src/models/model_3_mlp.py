# Model 3 - Multi-layer Perceptron (Context Window of Size 2 - Combined Approach)

import numpy as np
from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import SelectPercentile
from sklearn.model_selection import RandomizedSearchCV
from sklearn.model_selection._search import BaseSearchCV
from sklearn.neural_network import MLPClassifier
from sklearn.pipeline import Pipeline

from src.models.model_base_sklearn import SkLearnModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.transformers.feature_selector import FeatureSelector


class Model3MLP(SkLearnModelBase):
	feature_names = ['input_0_dataset_type', 'operation_0_type', 'input_1_dataset_type', 'operation_1_type']

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__(pipeline_client, dataset_client)

	def get_model_pipeline(self) -> BaseSearchCV:
		self.logger.debug("Creating model 3 pipeline for %s", __name__)
		params_clf = {
			'feature_selector__mark_missing_features': [True, False],
			'selector__percentile': np.linspace(start=10, stop=100, num=100),
			'classifier__hidden_layer_sizes': [(50, 50, 50), (50, 100, 50), (100,)],
			'classifier__activation': ['tanh', 'relu'],
			'classifier__solver': ['sgd', 'adam'],
			'classifier__alpha': [0.0001, 0.05],
			'classifier__learning_rate': ['constant', 'adaptive']
		}
		cv = 2
		ppl = Pipeline([
			('feature_selector', FeatureSelector(self.feature_names)),
			("encoder", DictVectorizer(sparse=False)),
			("selector", SelectPercentile()),
			("classifier", MLPClassifier(max_iter=1000))
		])
		return RandomizedSearchCV(ppl, params_clf, cv=cv, refit=True, n_jobs=-1, n_iter=25)

	def get_data(self, cache=True) -> (list, list):
		feat, lab = self._load_data(cache)
		return feat, lab
