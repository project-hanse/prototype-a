from sklearn import logger
from sklearn.base import BaseEstimator, TransformerMixin


class FeatureSelector(BaseEstimator, TransformerMixin):
	def __init__(self, feature_names: [str], mark_missing_features: bool = False):
		self.feature_names = feature_names
		self.mark_missing_features = mark_missing_features

	def fit(self, X, y=None):
		return self

	def transform(self, X, y=None):
		if self.feature_names is None or len(self.feature_names) == 0:
			logger.warning("No feature_names provided.")
			return X
		if hasattr(X, '__iter__'):
			for feat in X:
				self._select_features(feat, self.feature_names)
		else:
			self._select_features(X, self.feature_names)
		return X

	def _select_features(self, feat: {}, keys: [str]) -> {}:
		"""
		Selects features (by key) from the feature values array.
		If a value is not found, it is set to None.
		"""
		new_element = {}
		for key, val in feat.items():
			if key in keys:
				new_element[key] = val
			elif self.mark_missing_features:
				new_element[key] = '-1'
		return new_element
