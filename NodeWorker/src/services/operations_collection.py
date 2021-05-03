import json
import logging

import pandas as pd


class OperationsCollection:

    @staticmethod
    def simple_pd_generic(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation %s" % operation_name)

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
    def simple_pd_select_columns(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation %s" % operation_name)

        if isinstance(operation_config['0'], str):
            select_array = json.loads(operation_config['0'].replace('\'', '\"'))
        else:
            select_array = operation_config['0']

        resulting_dataset = df[select_array]

        logger.debug("Resulting dataset %s" % str(resulting_dataset))

        return resulting_dataset
