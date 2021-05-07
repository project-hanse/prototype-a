import json
import logging

import pandas as pd

from src.exceptions.ValidationError import ValidationError


class OperationsCollection:

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
    def pd_single_input_make_row_header(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        """
        Chooses a row and makes it the header of the df. Removes row from df and resets index.
        """
        logger.info("Executing pandas operation pd_single_input_make_row_header (%s)" % operation_name)
        if "header_row" not in operation_config:
            raise ValidationError("Missing header_row in config")

        header_row = operation_config["header_row"]

        df.columns = df.iloc[header_row]
        df = df.drop(header_row)
        df = df.reset_index(drop=True)
        return df

    @staticmethod
    def pd_single_input_trim_rows(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        """
        Removes the first and last n rows of a dataframe.
        """
        logger.info("Executing pandas operation pd_single_input_trim_rows (%s)" % operation_name)

        if "first_n" in operation_config:
            df = df.iloc[operation_config["first_n"]:]
        if "last_n" in operation_config:
            df = df.iloc[operation_config["last_n"]:]

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
