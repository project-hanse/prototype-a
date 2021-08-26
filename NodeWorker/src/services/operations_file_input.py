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

        header, index_col, names, skipfooter, skiprows, decimal = OperationsFileInputCollection.get_config(
            operation_config)

        if separator is 'auto':
            separator = OperationsFileInputCollection.get_sep(file_content)

        df = pd.read_csv(StringIO(file_content),
                         sep=separator,
                         skiprows=skiprows,
                         skipfooter=skipfooter,
                         header=header,
                         names=names,
                         decimal=decimal,
                         index_col=index_col)
        return df

    @staticmethod
    def pd_file_input_read_excel(logger: logging,
                                 operation_name: str,
                                 operation_config: dict,
                                 file_content) -> pd.DataFrame:
        """
        Read an Excel file into a pandas DataFrame. Supports xls, xlsx, xlsm, xlsb, odf, ods and odt file extensions
        read from a local filesystem or URL. Supports an option to read a single sheet or a list of sheets.

        https://pandas.pydata.org/docs/reference/api/pandas.read_excel.html
        """
        logger.info("Executing pandas operation pd_file_input_read_excel (%s)" % operation_name)

        header, index_col, names, skipfooter, skiprows = OperationsFileInputCollection.get_config(operation_config)

        df = pd.read_excel(file_content,
                           skiprows=skiprows,
                           skipfooter=skipfooter,
                           header=header,
                           names=names,
                           index_col=index_col)
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
        return header, index_col, names, skipfooter, skiprows, decimal

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
