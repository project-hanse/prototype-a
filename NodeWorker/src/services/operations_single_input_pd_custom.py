import logging

import pandas as pd


class OperationsSingleInputPandasCustom:

    @staticmethod
    def pd_single_input_set_date_index(logger: logging, operation_name: str, operation_config: dict, df: pd.DataFrame):
        logger.info("Executing pandas operation pd_single_input_set_date_index (%s)" % operation_name)

        if 'col_name' in operation_config:
            df['year'] = pd.DatetimeIndex(df[operation_config['col_name']]).year
            df['month'] = pd.DatetimeIndex(df[operation_config['col_name']]).month
            df['day'] = pd.DatetimeIndex(df[operation_config['col_name']]).day

        else:
            df['year'] = pd.DatetimeIndex(df.index).year
            df['month'] = pd.DatetimeIndex(df.index).month
            df['day'] = pd.DatetimeIndex(df.index).day

        return df.set_index(['year', 'month', 'day'])
