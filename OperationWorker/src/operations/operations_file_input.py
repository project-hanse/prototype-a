import io
import logging

import pandas as pd

from src.helper.operations_helper import OperationsHelper


class OperationsFileInputCollection:

	@staticmethod
	def pd_file_input_read_csv(logger: logging,
														 operation_name: str,
														 operation_config: dict,
														 data: [str]) -> pd.DataFrame:
		"""
		Loads a csv file and returns it as a dataframe.
		"""
		logger.info("Executing pandas operation pd_file_input_read_csv (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		if 'separator' in operation_config:
			separator = operation_config['separator']
		else:
			separator = 'auto'

		header, index_col, names, skipfooter, skiprows, decimal, parse_dates = OperationsFileInputCollection \
			.get_config(operation_config)

		if separator == 'auto':
			separator = OperationsFileInputCollection.get_sep(data[0])

		df = pd.read_csv(io.StringIO(data[0]),
										 sep=separator,
										 skiprows=skiprows,
										 skipfooter=skipfooter,
										 header=header,
										 names=names,
										 decimal=decimal,
										 index_col=index_col,
										 parse_dates=parse_dates)
		return df

	@staticmethod
	def pd_file_input_read_excel(logger: logging,
															 operation_name: str,
															 operation_config: dict,
															 data: []) -> pd.DataFrame:
		"""
		Read an Excel file into a pandas DataFrame. Supports xls, xlsx, xlsm, xlsb, odf, ods and odt file extensions
		read from a local filesystem or URL. Supports an option to read a single sheet or a list of sheets.

		https://pandas.pydata.org/docs/reference/api/pandas.read_excel.html
		"""
		logger.info("Executing pandas operation pd_file_input_read_excel (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		header, index_col, names, skipfooter, skiprows, decimal, parse_dates = OperationsFileInputCollection \
			.get_config(operation_config)

		df = pd.read_excel(data[0],
											 skiprows=skiprows,
											 skipfooter=skipfooter,
											 header=header,
											 names=names,
											 index_col=index_col,
											 parse_dates=parse_dates)
		return df

	@staticmethod
	def get_config(operation_config):
		if 'skiprows' in operation_config:
			skiprows = operation_config['skiprows']
		else:
			skiprows = None
		if 'skipfooter' in operation_config:
			skipfooter = operation_config['skipfooter']
		else:
			skipfooter = 0
		if 'header' in operation_config:
			header = operation_config['header']
		else:
			header = 0
		if 'names' in operation_config:
			names = operation_config['names']
		else:
			names = None
		if 'index_col' in operation_config:
			index_col = operation_config['index_col']
		else:
			index_col = None
		if 'decimal' in operation_config:
			decimal = operation_config['decimal']
		else:
			decimal = '.'
		if 'parse_dates' in operation_config:
			parse_dates = operation_config['parse_dates']
		else:
			parse_dates = False
		return header, index_col, names, skipfooter, skiprows, decimal, parse_dates

	@staticmethod
	def get_sep(file_content: str) -> str:
		"""
		Checks if a given file (assuming it's a csv file) is separated by , or ;
		"""
		# TODO: this should be made much more efficient but works for now
		try:
			df_comma = pd.read_csv(io.StringIO(file_content), nrows=1, sep=",")
			df_semi = pd.read_csv(io.StringIO(file_content), nrows=1, sep=";")
			if df_comma.shape[1] > df_semi.shape[1]:
				return ','
			else:
				return ';'
		except Exception as e:
			return ','
