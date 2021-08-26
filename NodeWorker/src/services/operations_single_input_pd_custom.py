import logging

import pandas as pd

from src.helper.operations_helper import OperationsHelper


class OperationsSingleInputPandasCustom:

    @staticmethod
    def pd_single_input_set_date_index(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_set_date_index (%s)" % operation_name)

        if 'col_name' in operation_config:
            df.index = pd.to_datetime(df['col_name'])

        else:
            df.index = pd.to_datetime(df.index.to_series())

        return df

    @staticmethod
    def pd_single_input_df_to_numeric(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_set_date_index (%s)" % operation_name)

        errors = OperationsHelper.get_or_default(operation_config, 'errors', 'raise')

        return df.apply(pd.to_numeric, errors=errors)

    @staticmethod
    def pd_single_input_groupby(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_groupby (%s)" % operation_name)

        by = OperationsHelper.get_or_default(operation_config, 'by', None)
        axis = OperationsHelper.get_or_default(operation_config, 'axis', 0)
        level = OperationsHelper.get_or_default(operation_config, 'level', None)
        as_index = OperationsHelper.get_or_default(operation_config, 'as_index', True)
        sort = OperationsHelper.get_or_default(operation_config, 'sort', True)
        group_keys = OperationsHelper.get_or_default(operation_config, 'group_keys', True)
        observed = OperationsHelper.get_or_default(operation_config, 'observed', False)
        dropna = OperationsHelper.get_or_default(operation_config, 'dropna', True)
        if 'group_by_operation' not in operation_config:
            raise ValueError('group_by_operation must be defined; Possible values: mean, sum, max, min')

        df_groupby = df.groupby(by=by,
                                axis=axis,
                                level=level,
                                as_index=as_index,
                                sort=sort,
                                group_keys=group_keys,
                                observed=observed,
                                dropna=dropna)
        if operation_config['group_by_operation'] is 'mean':
            return df_groupby.mean()
        if operation_config['group_by_operation'] is 'sum':
            return df_groupby.sum()
        if operation_config['group_by_operation'] is 'max':
            return df_groupby.max()
        if operation_config['group_by_operation'] is 'min':
            return df_groupby.min()

        raise ValueError('Configuration invalid')
