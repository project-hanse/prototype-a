import os
import tempfile


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
