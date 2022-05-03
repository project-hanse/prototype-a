from sklearn import logger
from sklearn.base import BaseEstimator, TransformerMixin


class FeatureSelector(BaseEstimator, TransformerMixin):
	def __init__(self, feature_names: [str], mark_missing_features: bool = False):
		self.feature_names = feature_names
		self.mark_missing_features = mark_missing_features

	def fit(self, X, y=None):
		return self

	def transform(self, X, y=None):
		if type(X) is not list:
			logger.error('X must be a list of dictionaries')
			return X
		return self._select_features(X, self.feature_names)

	def _select_features(self, feat: [{}], keys: [str]) -> [{}]:
		"""
		Selects features (by key) from the feature values array.
		If a value is not found, it is set to None.
		"""
		if keys is None or len(keys) == 0:
			return feat
		new_feat = []
		for element in feat:
			new_element = {}
			for key, val in element.items():
				if key in keys:
					new_element[key] = val
				elif self.mark_missing_features:
					new_element[key] = '-1'
			new_feat.append(new_element)
		return new_feat
