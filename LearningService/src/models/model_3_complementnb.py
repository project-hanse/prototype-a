import numpy as np
from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import SelectPercentile
from sklearn.model_selection._search import BaseSearchCV, RandomizedSearchCV
from sklearn.naive_bayes import ComplementNB
from sklearn.pipeline import Pipeline

from src.models.model_base_sklearn import SkLearnModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.transformers.dataset_types_to_category import DatasetTypesToCategory
from src.transformers.feature_selector import FeatureSelector


class Model3ComplementNB(SkLearnModelBase):
	feature_names = ['input_0_dataset_type', 'input_0_model_type', 'input_1_dataset_type', 'input_1_model_type',
									 'input_2_dataset_type', 'input_2_model_type', 'input_3_dataset_type', 'input_3_model_type',
									 'feat_pred_id', 'feat_pred_count', 'feat_pred_input_count']

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__(pipeline_client, dataset_client)

	def get_model_pipeline(self) -> BaseSearchCV:
		self.logger.debug("Creating model pipeline for %s", __name__)
		params = {
			'feature_selector__mark_missing_features': [True, False],
			'selector__percentile': np.linspace(start=10, stop=100, num=100),
			'classifier__alpha': np.linspace(0.1, 1.0, 50),
			'classifier__norm': [True, False],
			'classifier__fit_prior': [True, False]
		}
		cv = 2
		ppl = Pipeline([
			('datatype_to_category', DatasetTypesToCategory('dataset_type')),
			('feature_selector', FeatureSelector(self.feature_names)),
			("encoder", DictVectorizer(sparse=False)),
			("selector", SelectPercentile()),
			("classifier", ComplementNB())
		])
		return RandomizedSearchCV(ppl, params, cv=cv, refit=True, n_jobs=-1, n_iter=50)

	def get_data(self, cache=True) -> (list, list):
		feat, lab = self._load_data(cache)
		return feat, lab
