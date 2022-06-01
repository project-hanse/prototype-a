import logging

import pandas as pd

from src.helper.operations_helper import OperationsHelper


class OperationsDataConverter:

	@staticmethod
	def data_converter_df_to_dict(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Converts a dataframe to a list of dictionaries.
		"""
		logger.info("Executing data converter operation data_converter_df_to_dict (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		orient = OperationsHelper.get_or_default(operation_config, 'orientation', 'records')

		dicts = data[0].to_dict(orient=orient)

		return [dicts]

	@staticmethod
	def data_converter_np_array_to_df(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Converts a numpy array to a dataframe.
		"""

		logger.info("Executing data converter operation data_converter_np_array_to_df (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		return [pd.DataFrame(data[0])]
