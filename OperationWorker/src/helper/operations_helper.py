import os
import tempfile

import numpy as np


class OperationsHelper:
	tmp_dir = None

	@staticmethod
	def get_or_default(operation_config: dict, key: str, default):
		if key in operation_config:
			return operation_config[key]
		else:
			return default

	@classmethod
	def get_or_throw(cls, operation_config, key: str):
		if key in operation_config:
			return operation_config[key]
		else:
			raise ValueError("Configuration expected a value for key '%s'" % key)

	@classmethod
	def get_temporary_file_path(cls, dataset) -> str:
		if cls.tmp_dir is None:
			cls.tmp_dir = tempfile.mkdtemp()

		return os.path.join(cls.tmp_dir, dataset.get_key())

	@classmethod
	def validate_input_or_throw(cls, data, expected_length: int):
		if len(data) != expected_length:
			raise ValueError("Expected %i input datasets but got %i" % (expected_length, len(data)))

	@classmethod
	def to_np_array(cls, transformed):
		if not isinstance(transformed, np.ndarray):
			transformed = transformed.toarray()
		return transformed
