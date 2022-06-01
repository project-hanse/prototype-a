import logging

import numpy as np
from sklearn.feature_extraction import DictVectorizer

from src.helper.operations_helper import OperationsHelper


class OperationsSklearnDictVectorizer:

	@staticmethod
	def sklearn_dict_vectorizer(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Creates a DictVectorizer to encode categorical features as a one-hot numeric array.

		This transformer should be used to encode categorical features.
		https://scikit-learn.org/stable/modules/generated/sklearn.feature_extraction.DictVectorizer.html
		"""
		logger.info("Executing scikit operation sklearn_dict_vectorizer (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 0)
		OperationsHelper.get_or_default(operation_config, "sparse", False)
		OperationsHelper.get_or_default(operation_config, "dtype", np.float64)
		OperationsHelper.get_or_default(operation_config, "sort", True)
		OperationsHelper.get_or_default(operation_config, "separator", '=')
		return DictVectorizer()

	@staticmethod
	def sklearn_dict_vectorizer_fit(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Calls fit with the provided data on the provided dict vectorizer.
		"""
		logger.info("Executing scikit operation sklearn_dict_vectorizer_fit (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)

		encoder = data[0]
		encoder.fit(data[1])

		return [encoder]

	@staticmethod
	def sklearn_dict_vectorizer_fit_transform(logger: logging, operation_name: str, operation_config: dict,
																						data: []) -> []:
		"""
		Calls fit and transform on the provided dict vectorizer.
		"""
		logger.info("Executing scikit operation sklearn_dict_vectorizer_fit_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)

		encoder = data[0]
		transformed = encoder.fit_transform(data[1])
		transformed = OperationsHelper.to_np_array(transformed)
		return [encoder, transformed]

	@staticmethod
	def sklearn_dict_vectorizer_transform(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Calls transform on the provided dict vectorizer.
		"""
		logger.info("Executing scikit operation sklearn_dict_vectorizer_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)

		encoder = data[0]
		transformed = encoder.transform(data[1])

		transformed = OperationsHelper.to_np_array(transformed)

		return [transformed]

	@staticmethod
	def sklearn_dict_vectorizer_inverse_transform(logger: logging, operation_name: str, operation_config: dict,
																								data: []) -> []:
		"""
		Calls inverse_transform on the provided dict vectorizer.
		"""
		logger.info("Executing scikit operation sklearn_dict_vectorizer_inverse_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)

		encoder = data[0]
		inverse_transformed = encoder.inverse_transform(data[1])
		return [inverse_transformed]
