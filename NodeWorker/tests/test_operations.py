import logging
import unittest

from src.services.operations_collection import OperationsCollection
from tests.helper.import_helper import load_file


class OperationsUnitTests(unittest.TestCase):
    logger: logging

    def __init__(self, methodName: str = ...) -> None:
        super().__init__(methodName)
        self.logger = logging

    def test_pd_drop_by_index(self):
        # arrange
        df = load_file("./datasets/ZAMG_Jahrbuch_1990-utf-8.csv")

        # act
        result = OperationsCollection.pd_single_input_drop(
            self.logger,
            "drop",
            {
                "labels": ['Beaufort'],
                "axis": "index"
            },
            df)

        # assert
        self.assertIsNotNone(result)
        # TODO add proper asserts


if __name__ == '__main__':
    unittest.main()
