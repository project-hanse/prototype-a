import warnings

from sklearn.feature_selection import SelectKBest


class PipelineSelectKBest(SelectKBest):
	"""
	SelectKBest is a wrapper for sklearn's SelectKBest.
	Based https://stackoverflow.com/a/62357269/11016410.
	"""

	def _check_params(self, X, y):
		if self.k >= X.shape[1]:
			warnings.warn("Less than %d number of features found, so setting k as %d" % (self.k, X.shape[1]), UserWarning)
			self.k = X.shape[1]
		if not (self.k == "all" or 0 <= self.k):
			raise ValueError(
				"k should be >=0, <= n_features = %d; got %r. Use k='all' to return all features." % (X.shape[1], self.k))
