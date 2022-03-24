from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import f_classif
from sklearn.model_selection import GridSearchCV
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

	def get_model_pipeline(self) -> Pipeline:
		self.logger.debug("Creating model pipeline for %s", __name__)
		params = {
			'alpha': [0.1, 0.25, 0.5, 0.75, 1.0, 1.5, 2.0, 5.0, 7.5, 10.0],
			'norm': [True, False],
			'fit_prior': [True, False]
		}
		k = 24
		cv = 2
		ppl = Pipeline([
			("vectorizer", DictVectorizer(sparse=False)),
			("selector", PipelineSelectKBest(f_classif, k=k)),
			("classifier", GridSearchCV(
				ComplementNB(),
				param_grid=params,
				cv=cv,
				refit=True,
				n_jobs=-1))
		])
		return ppl

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
		feat = self._whitelist_features(feat, self.feature_names)
		return feat, lab
