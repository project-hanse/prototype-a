import json
import logging

import pandas as pd

from src.exceptions.ValidationError import ValidationError
from src.helper.operations_helper import OperationsHelper


class OperationsSingleInputPandasWrappers:
	"""
	Primarily wrappers around pandas operations.
	"""

	@staticmethod
	def pd_single_input_generic(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		logger.info("Executing pandas operation pd_single_input_generic (%s)" % operation_name)

		command = ("resulting_dataset = df.%s(**operation_config)" % operation_name)
		# TODO: whitelist of allowed operations
		loc = {'df': df, 'operation_config': operation_config}
		exec(command, globals(), loc)
		resulting_dataset = loc['resulting_dataset']

		# TODO: remove this conversion once pd.Series is supported as datatype
		if isinstance(resulting_dataset, pd.Series):
			logger.info('Converting series to dataframe')
			resulting_dataset = resulting_dataset.to_frame()

		logger.debug("Resulting dataset \n%s" % str(resulting_dataset))

		return resulting_dataset

	@staticmethod
	def pd_single_input_set_index(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Sets the DataFrame index using existing columns.
		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.set_index.html
		"""
		logger.info("Executing pandas operation pd_single_input_set_index (%s)" % operation_name)
		if "keys" not in operation_config:
			raise ValidationError("Missing keys in config")
		keys = operation_config["keys"]
		if "drop" not in operation_config:
			drop = True
		else:
			drop = operation_config["drop"]

		return df.set_index(keys, drop=drop)

	@staticmethod
	def pd_single_input_reset_index(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Reset the index, or a level of it. Reset the index of the DataFrame, and use the default one instead. If the
		DataFrame has a MultiIndex, this method can remove one or more levels.

		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.reset_index.html
		"""
		logger.info("Executing pandas operation pd_single_input_set_index (%s)" % operation_name)
		if "level" in operation_config:
			level = operation_config["level"]
		else:
			level = None

		if "drop" in operation_config:
			drop = operation_config["drop"]
		else:
			drop = False

		if "inplace" in operation_config:
			inplace = operation_config['inplace']
		else:
			inplace = False

		if "col_level" in operation_config:
			col_level = operation_config['col_level']
		else:
			col_level = 0

		if "col_fill" in operation_config:
			col_fill = operation_config['col_fill']
		else:
			col_fill = 0

		return df.reset_index(level=level, drop=drop, inplace=inplace, col_level=col_level, col_fill=col_fill)

	@staticmethod
	def pd_single_input_transpose(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
						Transpose index and columns.
						Reflect the DataFrame over its main diagonal by writing rows as columns and vice-versa.
						https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.transpose.html
						"""
		logger.info("Executing pandas operation pd_single_input_transpose (%s)" % operation_name)

		return df.T

	@staticmethod
	def pd_single_input_rename(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Alters axes labels of a DataFrame.
		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.rename.html
		"""
		logger.info("Executing pandas operation pd_single_input_rename (%s)" % operation_name)
		if "mapper" not in operation_config:
			raise ValidationError("Missing mapper in config")
		mapper = operation_config["mapper"]
		if "axis" not in operation_config:
			axis = 'columns'
		else:
			axis = operation_config["axis"]

		return df.rename(mapper, axis=axis)

	@staticmethod
	def pd_single_input_drop(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Drop specified labels from rows or columns.
		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.drop.html
		"""
		logger.info("Executing pandas operation pd_single_input_drop (%s)" % operation_name)

		if "labels" not in operation_config["labels"]:
			labels = operation_config["labels"]
		else:
			labels = None

		if "axis" not in operation_config:
			axis = 0
		else:
			axis = operation_config["axis"]

		df.drop(labels=labels, axis=axis, inplace=True)
		return df

	@staticmethod
	def pd_single_input_mean(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Return the mean of the values over the requested axis.
		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.DataFrame.mean.html
		"""
		logger.info("Executing pandas operation pd_single_input_mean (%s)" % operation_name)

		if "axis" not in operation_config:
			axis = 1
		else:
			axis = operation_config["axis"]

		if "skipna" not in operation_config:
			skipna = True
		else:
			skipna = operation_config["skipna"]

		if "level" not in operation_config:
			level = None
		else:
			level = operation_config["level"]

		if "numeric_only" not in operation_config:
			numeric_only = None
		else:
			numeric_only = operation_config["numeric_only"]

		if "name" not in operation_config:
			name = "mean"
		else:
			name = operation_config["name"]

		return df.mean(axis=axis, skipna=skipna, level=level, numeric_only=numeric_only).to_frame(name=name)

	@staticmethod
	def pd_single_input_make_row_header(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Chooses a row and makes it the header of the df. Removes row from df and resets index.
		"""
		logger.info("Executing pandas operation pd_single_input_make_row_header (%s)" % operation_name)
		if "header_row" not in operation_config:
			raise ValidationError("Missing header_row in config")

		header_row = operation_config["header_row"]

		if type(header_row) == int:
			df.columns = df.iloc[header_row]
			df.drop(labels=header_row, axis='index', inplace=True)
		else:
			df.columns = df.loc[header_row]
			df.drop(header_row, inplace=True)
		return df

	@staticmethod
	def pd_single_input_trim_rows(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Removes the first and last n rows of a dataframe.
		"""
		logger.info("Executing pandas operation pd_single_input_trim_rows (%s)" % operation_name)

		if "first_n" in operation_config:
			df = df.iloc[operation_config["first_n"] - 1:]
		if "last_n" in operation_config:
			df = df.iloc[:operation_config["last_n"]]

		return df

	@staticmethod
	def pd_single_input_replace(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Replace values given in to_replace with value.
		Values of the DataFrame are replaced with other values dynamically.

		https://pandas.pydata.org/docs/reference/api/pandas.DataFrame.replace.html
		"""
		logger.info("Executing pandas operation pd_single_input_replace (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 1)

		to_replace = OperationsHelper.get_or_default(operation_config, 'to_replace', None)
		value = OperationsHelper.get_or_default(operation_config, 'value', None)

		return [data[0].replace(to_replace=to_replace, value=value)]

	@staticmethod
	def pd_single_input_interpolate(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Fill NaN values using an interpolation method.
		Please note that only method='linear' is supported for DataFrame/Series with a MultiIndex.

		https://pandas.pydata.org/docs/reference/api/pandas.DataFrame.interpolate.html
		"""
		logger.info("Executing pandas operation pd_single_input_interpolate (%s)" % operation_name)

		method = OperationsHelper.get_or_default(operation_config, 'method', 'linear')
		axis = OperationsHelper.get_or_default(operation_config, 'axis', None)
		limit = OperationsHelper.get_or_default(operation_config, 'limit', None)
		limit_direction = OperationsHelper.get_or_default(operation_config, 'limit_direction', None)
		limit_area = OperationsHelper.get_or_default(operation_config, 'limit_area', None)
		downcast = OperationsHelper.get_or_default(operation_config, 'downcast', None)

		return df.interpolate(method=method, axis=axis, limit=limit, limit_direction=limit_direction,
													limit_area=limit_area, downcast=downcast)

	@staticmethod
	def pd_single_input_select_columns(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		logger.info("Executing pandas operation pd_single_input_select_columns (%s)" % operation_name)

		if isinstance(operation_config['columns'], str):
			select_array = json.loads(operation_config['columns'].replace('\'', '\"'))
		else:
			select_array = operation_config['columns']

		resulting_dataset = df[select_array]

		logger.debug("Resulting dataset %s" % str(resulting_dataset))

		return resulting_dataset

	@staticmethod
	def pd_single_input_select_rows(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Selects only rows where a value of a column matches a given value.
		"""
		logger.info("Executing pandas operation pd_single_input_select_rows (%s)" % operation_name)

		if "column_name" not in operation_config:
			raise ValidationError("Missing column_name in config")
		if "select_value" not in operation_config:
			raise ValidationError("Missing select_value in config")

		return df.loc[df[operation_config["column_name"]] == operation_config["select_value"]]

	@staticmethod
	def pd_single_input_sort_index(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
		"""
		Sort object by labels (along an axis). Returns a new DataFrame sorted by label.
		https://pandas.pydata.org/docs/reference/api/pandas.DataFrame.sort_index.html
		"""
		logger.info("Executing pandas operation pd_single_input_sort_index (%s)" % operation_name)

		if "axis" in operation_config:
			axis = operation_config['axis']
		else:
			axis = 0

		if "level" in operation_config:
			level = operation_config['level']
		else:
			level = None

		if "ascending" in operation_config:
			ascending = operation_config['ascending']
		else:
			ascending = True

		return df.sort_index(axis=axis, level=level, ascending=ascending)

	@staticmethod
	def pd_double_input_join(logger: logging,
													 operation_name: str,
													 operation_config: dict,
													 df_one: pd.DataFrame,
													 df_two: pd.DataFrame):
		"""
		Joins dataset two onto dataset one.
		"""
		logger.info("Executing pandas operation pd_double_input_join (%s)" % operation_name)

		if "on" in operation_config:
			join_on = operation_config["on"]
		else:
			join_on = None

		if "lsuffix" in operation_config:
			lsuffix = operation_config["lsuffix"]
		else:
			lsuffix = ''

		if "rsuffix" in operation_config:
			rsuffix = operation_config["rsuffix"]
		else:
			rsuffix = ''

		# TODO: add prefix options
		if join_on is None:
			df_one_reindex = df_one
			df_two_reindex = df_two
		else:
			df_one_reindex = df_one.set_index(join_on)
			df_two_reindex = df_two.set_index(join_on)

		return df_one_reindex.join(df_two_reindex, lsuffix=lsuffix, rsuffix=rsuffix)

	@staticmethod
	def pd_double_input_concat(logger: logging,
														 operation_name: str,
														 operation_config: dict,
														 df_one: pd.DataFrame,
														 df_two: pd.DataFrame):
		"""
		Concatenate pandas objects along a particular axis with optional set logic along the other axes.
		https://pandas.pydata.org/pandas-docs/stable/reference/api/pandas.concat.html#pandas.concat
		"""
		logger.info("Executing pandas operation pd_double_input_concat (%s)" % operation_name)

		if "axis" in operation_config:
			axis = operation_config["axis"]
		else:
			axis = 0

		if "join" in operation_config:
			join = operation_config["join"]
		else:
			join = 'outer'

		if "ignore_index" in operation_config:
			ignore_index = operation_config["ignore_index"]
		else:
			ignore_index = False

		return pd.concat([df_one, df_two], axis=axis, join=join, ignore_index=ignore_index)
