import json
import logging

import pandas as pd

from src.exceptions.ValidationError import ValidationError


class OperationsSingleInputCollection:
    @staticmethod
    def pd_single_input_generic(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_generic (%s)" % operation_name)

        # TODO: Catch and handle exceptions
        if operation_name == 'select_columns':
            resulting_dataset = df[dict[0]]
        else:
            command = ("resulting_dataset = df.%s(**operation_config)" % operation_name)
            loc = {
                'df': df,
                'operation_config': operation_config
            }
            exec(command, globals(), loc)
            resulting_dataset = loc['resulting_dataset']

        logger.debug("Resulting dataset %s" % str(resulting_dataset))

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
    def pd_single_input_select_columns(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_select_columns (%s)" % operation_name)

        if isinstance(operation_config['0'], str):
            select_array = json.loads(operation_config['0'].replace('\'', '\"'))
        else:
            select_array = operation_config['0']

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
    def pd_double_input_join(logger: logging,
                             operation_name: str,
                             operation_config: dict,
                             df_one: pd.DataFrame,
                             df_two: pd.DataFrame):
        """
        Joins dataset two onto dataset one.
        """
        logger.info("Executing pandas operation pd_double_input_join (%s)" % operation_name)

        if "on" not in operation_config:
            raise ValidationError("Missing on in config")
        join_on = operation_config["on"]

        if "lsuffix" in operation_config:
            lsuffix = operation_config["lsuffix"]
        else:
            lsuffix = "_left"

        if "rsuffix" in operation_config:
            rsuffix = operation_config["rsuffix"]
        else:
            rsuffix = "_right"

        # TODO: add prefix options

        df_one_reindex = df_one.set_index(join_on)
        df_two_reindex = df_two.set_index(join_on)

        return df_one_reindex.join(df_two_reindex, lsuffix=lsuffix, rsuffix=rsuffix)
