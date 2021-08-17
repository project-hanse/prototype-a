import logging
from io import StringIO

import pandas as pd


class OperationsFileInputCollection:

    @staticmethod
    def pd_file_input_read_csv(logger: logging,
                               operation_name: str,
                               operation_config: dict,
                               file_content: str) -> pd.DataFrame:
        """
        Loads a csv file and returns it as a dataframe.
        """
        logger.info("Executing pandas operation pd_file_input_read_csv (%s)" % operation_name)

        if 'separator' in operation_config:
            separator = operation_config['separator']
        else:
            separator = 'auto'

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

        if separator is 'auto':
            separator = OperationsFileInputCollection.get_sep(file_content)

        df = pd.read_csv(StringIO(file_content),
                         sep=separator,
                         skiprows=skiprows,
                         skipfooter=skipfooter,
                         header=header,
                         names=names,
                         index_col=index_col)
        return df

    @staticmethod
    def get_sep(file_content: str):
        """
        Checks if a given file (assuming it's a csv file) is separated by , or ;
        """
        # TODO: this should be made much more efficient but works for now
        df_comma = pd.read_csv(StringIO(file_content), nrows=1, sep=",")
        df_semi = pd.read_csv(StringIO(file_content), nrows=1, sep=";")
        if df_comma.shape[1] > df_semi.shape[1]:
            return ','
        else:
            return ';'
