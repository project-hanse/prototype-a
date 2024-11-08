# Model 1 - Naive Bayes (Context Window of Size 1 - Only Dataset Nodes)

import numpy as np
from sklearn.feature_extraction import DictVectorizer
from sklearn.feature_selection import SelectPercentile
from sklearn.model_selection import RandomizedSearchCV
from sklearn.model_selection._search import BaseSearchCV
from sklearn.naive_bayes import ComplementNB
from sklearn.pipeline import Pipeline

from src.models.model_base_sklearn import SkLearnModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.transformers.dataset_types_to_category import DatasetTypesToCategory
from src.transformers.feature_selector import FeatureSelector


class Model1NaiveBayes(SkLearnModelBase):
    feature_names = None

    def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
        super().__init__(pipeline_client, dataset_client)
        self.feature_names = self.feat_model_1

    def get_model_pipeline(self) -> BaseSearchCV:
        self.logger.debug("Creating model 1 pipeline for %s", __name__)
        params_clf = {
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
        return RandomizedSearchCV(ppl, params_clf, cv=cv, refit=True, n_jobs=-1, n_iter=25)

    def get_data(self, cache=True) -> (list, list):
        feat, lab = self._load_data(cache)
        return feat, lab
