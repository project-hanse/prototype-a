import logging

import pandas as pd
from sklearn.preprocessing import LabelEncoder

from src.helper.operations_helper import OperationsHelper


class OperationsSklearnPreprocessingWrappers:

	@staticmethod
	def sklearn_label_encoder(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Creates a LabelEncoder to encode target labels with value between 0 and n_classes-1.

		This transformer should be used to encode target values, i.e. y, and not the input X.
		https://scikit-learn.org/stable/modules/generated/sklearn.preprocessing.LabelEncoder.html
		"""
		logger.info("Executing scikit operation sklearn_label_encoder (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 0)
		return LabelEncoder()

	@staticmethod
	def sklearn_encoder_fit(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Calls fit on the provided data on the provided encoder.
		"""
		logger.info("Executing scikit operation sklearn_encoder_fit (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)
		encoder = data[0]
		encoder.fit(data[1])

		return [encoder]

	@staticmethod
	def sklearn_encoder_transform(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Calls transform on the provided data on the provided encoder.
		"""
		logger.info("Executing scikit operation sklearn_encoder_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)
		encoder = data[0]
		encoded = pd.Series(encoder.transform(data[1]), index=data[1].index, name=data[1].name)

		return [encoded]

	@staticmethod
	def sklearn_encoder_fit_transform(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Takes an encoder to encode target labels with value between 0 and n_classes-1.
		Calls fit and transform in one step on the provided data.

		This transformer should be used to encode target values, i.e. y, and not the input X.
		https://scikit-learn.org/stable/modules/generated/sklearn.preprocessing.LabelEncoder.html
		"""
		logger.info("Executing scikit operation sklearn_label_encoder_fit_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)
		encoder = data[0]
		encoded = pd.Series(encoder.fit_transform(data[1]), index=data[1].index, name=data[1].name)

		return [encoder, encoded]

	@staticmethod
	def sklearn_encoder_inverse_transform(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Takes an encoder, encoded data and returns the original data.

		https://scikit-learn.org/stable/modules/generated/sklearn.preprocessing.LabelEncoder.html
		"""
		logger.info("Executing scikit operation sklearn_encoder_inverse_transform (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 2)
		encoder = data[0]
		decoded = encoder.inverse_transform(data[1])

		return [pd.Series(decoded, index=data[1].index, name=data[1].name)]
