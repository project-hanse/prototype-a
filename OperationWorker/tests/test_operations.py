import logging
import unittest

from src.services.operations_single_input_pd_wrappers import OperationsSingleInputPandasWrappers
from tests.helper.import_helper import load_file


class OperationsUnitTests(unittest.TestCase):
    logger: logging

    def __init__(self, methodName: str = ...) -> None:
        super().__init__(methodName)
        self.logger = logging

    def test_pd_artificial_drop_by_cols(self):
        # arrange
        df = load_file("./datasets/artificial-1.csv", ',')

        # act
        result = OperationsSingleInputPandasWrappers.pd_single_input_drop(
            self.logger,
            "drop",
            {
                "labels": ['A'],
                "axis": "columns"
            },
            df)

        # assert
        self.assertIsNotNone(result)
        self.assertEqual(result.columns.values.tolist(), ['B', 'C', 'D', 'E'])

    def test_pd_artificial_drop_by_index(self):
        # arrange
        df = load_file("./datasets/artificial-1.csv", ',')

        # act
        result = OperationsSingleInputPandasWrappers.pd_single_input_drop(
            self.logger,
            "drop",
            {
                "labels": [1]
            },
            df)

        # assert
        self.assertIsNotNone(result)
        self.assertEqual(result.index.values.tolist(), [0, 2, 3])

    def test_pd_weather_drop_by_index(self):
        # arrange
        df = load_file("./datasets/ZAMG_Jahrbuch_1990-utf-8.csv", csv_sep=';', skip_rows=4)

        # act
        result = OperationsSingleInputPandasWrappers.pd_single_input_drop(
            self.logger,
            "drop",
            {
                "labels": [0, 1, 2, 3]
            },
            df)

        # assert
        self.assertIsNotNone(result)
        # self.assertEqual(result.index.values.tolist(), [0, 2, 3])

    def test_pd_select_columns(self):
        # arrange
        df = load_file("./datasets/artificial-1.csv", ',')

        # act
        result = OperationsSingleInputPandasWrappers.pd_single_input_select_columns(
            self.logger,
            "select_columns",
            {
                '0': ['A', 'B']
            },
            df)

        # assert
        self.assertIsNotNone(result)
        self.assertEqual(result.columns.values.tolist(), ['A', 'B'])


if __name__ == '__main__':
    unittest.main()
