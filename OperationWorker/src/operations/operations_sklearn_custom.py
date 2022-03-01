import logging
import random

import numpy as np
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
	def sklearn_split(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Get a split of data from a dataframe.
		"""
		logger.info("Executing scikit operation sklearn_get_split (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		random_state = OperationsHelper.get_or_default(operation_config, 'random_state', random.randint(0, 1000))
		split_size = OperationsHelper.get_or_default(operation_config, 'split_size', 0.8)
		shuffle = OperationsHelper.get_or_default(operation_config, 'shuffle', True)

		split_train, _ = train_test_split(data[0], train_size=split_size, random_state=random_state, shuffle=shuffle)

		return split_train

	@staticmethod
	def min_max_scaling(column):
		return (column - column.min()) / (column.max() - column.min())

	@staticmethod
	def sklearn_transform(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Transform columns of a dataframe.
		"""
		logger.info("Executing scikit operation sklearn_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)
		columns = OperationsHelper.get_or_default(operation_config, 'columns', data[0].columns)
		min_max = OperationsHelper.get_or_default(operation_config, 'min_max', False)
		log = OperationsHelper.get_or_default(operation_config, 'log', False)

		if columns is None:
			columns = data[0].columns

		df = data[0]

		for col in columns:
			x = np.array(df[col])
			if min_max:
				x = OperationsSklearnCustom.min_max_scaling(x)
			if log:
				x = np.log(x + 1)
			df[col] = x

		return df

	@staticmethod
	def sklearn_extract_features(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Removes the target column from the dataframe.
		"""
		logger.info("Executing scikit operation sklearn_extract_labels (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		df = data[0]

		target_column = OperationsHelper.get_or_default(operation_config, 'target_column', "target")

		if target_column not in df.columns:
			raise ValueError("Column %s not found in dataframe" % target_column)

		return df.drop(columns=[target_column])

	@staticmethod
	def sklearn_extract_targets(logger: logging, operation_name: str, operation_config: dict, data: []):
		"""
		Extracts a target column from a dataframe.
		"""
		logger.info("Executing scikit operation sklearn_extract_labels (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		df = data[0]

		target_column = OperationsHelper.get_or_default(operation_config, 'target_column', "target")

		if target_column not in df.columns:
			raise ValueError("Column %s not found in dataframe" % target_column)

		labels = df[target_column]

		return labels
