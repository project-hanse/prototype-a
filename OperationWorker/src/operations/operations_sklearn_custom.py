import logging
import random

import pandas as pd
from sklearn.model_selection import train_test_split
from sklearn.neural_network import MLPRegressor

from src.helper.operations_helper import OperationsHelper


class OperationsSklearnCustom:

	@staticmethod
	def sklearn_double_input_predict(logger: logging,
																	 operation_name: str,
																	 operation_config: dict,
																	 df_one: pd.DataFrame,
																	 df_two: pd.DataFrame):
		"""
		TODO: this as actually a custom implementation
		Multi-layer Perceptron regressor.
		This model optimizes the squared-loss using LBFGS or stochastic gradient descent.

		https://scikit-learn.org/stable/modules/generated/sklearn.neural_network.MLPRegressor.htm
		"""
		logger.info("Executing scikit operation scikit_single_input_mlp_regressor (%s)" % operation_name)

		hidden_layer_sizes = OperationsHelper.get_or_default(operation_config, 'hidden_layer_sizes', (100,))
		max_iter = OperationsHelper.get_or_default(operation_config, 'max_iter', 200)

		X_train, X_test, y_train, y_test = train_test_split(df_one, df_two)

		regr = MLPRegressor(max_iter=max_iter, hidden_layer_sizes=hidden_layer_sizes)
		model = regr.fit(X_train, y_train)
		prediction = model.predict(X_test[:1])
		score = model.score(X_test, y_test)

		logger.info("Prediction score %s" % str(score))

		df = pd.DataFrame(data=prediction.tolist(),
											columns=df_two.columns)

		# Sets the index of the predicted dataframe
		df.index = X_test.index[:1]

		return df

	@staticmethod
	def sklearn_get_split(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Get a split of data from a dataframe.
		"""
		logger.info("Executing scikit operation sklearn_get_split (%s)" % operation_name)

		random_state = OperationsHelper.get_or_default(operation_config, 'random_state', random.randint(0, 1000))
		split_size = OperationsHelper.get_or_default(operation_config, 'split_size', 0.8)
		shuffle = OperationsHelper.get_or_default(operation_config, 'shuffle', True)

		split_train, _ = train_test_split(data[0], train_size=split_size, random_state=random_state, shuffle=shuffle)

		return split_train
