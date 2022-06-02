from sklearn.base import BaseEstimator, TransformerMixin


class DatasetTypesToCategory(BaseEstimator, TransformerMixin):
	def __init__(self, type_indicator: str = 'dataset_type'):
		self.type_indicator = type_indicator

	def fit(self, X, y=None):
		return self

	def transform(self, X, y=None):
		if type(X) is dict:
			self._convert_datatype_to_string(X, self.type_indicator)
		elif hasattr(X, '__iter__'):
			for feat in X:
				self._convert_datatype_to_string(feat, self.type_indicator)
		return X

	def _convert_datatype_to_string(self, feat: {}, key_indicator: str):
		"""
		Looks for keys containing the type indicator and converts the value to a string.
		"""
		if type(feat) is not dict:
			return feat
		for key, val in feat.items():
			if key_indicator in key:
				feat[key] = str(val)
