import logging

import pandas as pd
from prophet import Prophet

from src.helper.operations_helper import OperationsHelper


class OperationsProphet:

	@staticmethod
	def prophet_fit(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame) -> Prophet:
		logger.info("Executing prophet operation prophet_fit (%s)" % operation_name)

		m = Prophet()
		m.fit(df)

		return m

	@staticmethod
	def prophet_make_future_dataframe(logger: logging, operation_name: str, operation_config: dict,
																		m: Prophet) -> pd.DataFrame:
		logger.info("Executing prophet operation prophet_make_future_dataframe (%s)" % operation_name)

		periods = OperationsHelper.get_or_default(operation_config, 'periods', 365)
		freq = OperationsHelper.get_or_default(operation_config, 'freq', 'D')
		include_history = OperationsHelper.get_or_default(operation_config, 'include_history', True)

		future = m.make_future_dataframe(periods=periods, freq=freq, include_history=include_history)

		return future

	@staticmethod
	def prophet_predict(logger: logging, operation_name: str, operation_config: dict, m: Prophet,
											future: pd.DataFrame) -> pd.DataFrame:
		logger.info("Executing prophet operation prophet_predict (%s)" % operation_name)

		forecast = m.predict(future)

		return forecast
