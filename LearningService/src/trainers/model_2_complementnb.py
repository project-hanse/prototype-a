import numpy as np
from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import f_classif
from sklearn.model_selection._search import BaseSearchCV, RandomizedSearchCV
from sklearn.naive_bayes import ComplementNB
from sklearn.pipeline import Pipeline

from src.helper.select_K_best import PipelineSelectKBest
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.trainers.model_base import TrainerModelBase


class TrainerModel2ComplementNB(TrainerModelBase):
	feature_names = []

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__(pipeline_client, dataset_client)

	def get_model_pipeline(self) -> BaseSearchCV:
		self.logger.debug("Creating model 2 pipeline for %s", __name__)
		# need k greater than just number of features since DictVectorizer will create a feature for each key
		params = {
			'selector__k': np.linspace(1, len(self.feature_names) * 30, 50, dtype=int),
			'classifier__alpha': np.linspace(0.1, 1.0, 50),
			'classifier__norm': [True, False],
			'classifier__fit_prior': [True, False]
		}
		cv = 2
		ppl = Pipeline([
			("encoder", DictVectorizer(sparse=False)),
			("selector", PipelineSelectKBest(f_classif)),
			("classifier", ComplementNB())
		])
		return RandomizedSearchCV(ppl, params, cv=cv, refit=True, n_jobs=-1, n_iter=30)

	def _tuples_preprocessing(self, data: []) -> ([{}], [{}]):
		feat = []
		lab = []
		for element in data:
			lab.append(element['targetOperationIdentifier'])
			feat.append({
				'feat_pred_id': element['predecessorOperationIdentifier'],
				'feat_pred_input_count': len(element['targetInputs'])
			})

		return feat, lab

	def get_data(self, cache=True) -> (list, list):
		feat, lab = self._load_data(cache)
		feat = self._select_features(feat, self.feature_names)
		return feat, lab
