import logging
import unittest

import pandas as pd

from src.operations.operations_sklearn_custom import OperationsSklearnCustom


class OperationsSklearnCustomUnitTests(unittest.TestCase):
	logger: logging

	def __init__(self, methodName: str = ...) -> None:
		super().__init__(methodName)
		self.logger = logging

	def test_sklearn_split(self):
		# arrange
		df = pd.DataFrame(range(0, 100), columns=['Number'])
		result = OperationsSklearnCustom.sklearn_get_split(self.logger, "split", {
			'split_size': 0.8
		}, [df])

		# assert
		self.assertIsNotNone(result)
		self.assertEqual(type(df), type(result))
		self.assertEqual(len(result), 80)


if __name__ == '__main__':
	unittest.main()
