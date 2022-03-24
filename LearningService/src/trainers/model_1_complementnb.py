from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import f_classif
from sklearn.model_selection import GridSearchCV
from sklearn.naive_bayes import ComplementNB
from sklearn.pipeline import Pipeline

from src.helper.select_K_best import PipelineSelectKBest
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.trainers.model_1_base import TrainerModel1Base


class TrainerModel1ComplementNB(TrainerModel1Base):
	feature_names = ['type', 'input_0_dataType', 'input_1_dataType', 'input_2_dataType', 'model_type']

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		super().__init__(pipeline_client, dataset_client)

	def get_model_pipeline(self) -> Pipeline:
		self.logger.debug("Creating model pipeline for %s", __name__)
		params = {
			'alpha': [0.1, 0.5, 1.0, 2.0, 5.0, 7.5, 10.0],
			'norm': [True, False],
			'fit_prior': [True, False]
		}
		k = 16
		cv = 2
		ppl = Pipeline([
			("vectorizer", DictVectorizer(sparse=False)),
			("selector", PipelineSelectKBest(f_classif, k=k)),
			("hyper_param_tuning", GridSearchCV(
				ComplementNB(),
				param_grid=params,
				cv=cv,
				refit=True,
				n_jobs=-1))
		])
		return ppl

	def get_data(self, cache=True) -> (list, list):
		feat, lab = self._load_data(cache)
		feat = self._whitelist_features(feat, self.feature_names)
		return feat, lab
