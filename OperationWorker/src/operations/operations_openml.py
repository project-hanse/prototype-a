import logging
import os

from sklearn.datasets import fetch_openml

from src.constants.dirs import open_ml_datasets_local_copy
from src.helper.operations_helper import OperationsHelper


class OperationsOpenML:
	def __init__(self):
		pass

	@staticmethod
	def load_data_from_openml(logger: logging, operation_name: str, operation_config: dict, data: []) -> []:
		"""
		Fetch dataset from openml by name or dataset id.
		"""
		logger.info("Executing openml operation load_data_from_openml (%s)" % operation_name)

		OperationsHelper.validate_input_or_throw(data, 0)
		name = OperationsHelper.get_or_default(operation_config, 'name', None)
		version = OperationsHelper.get_or_default(operation_config, 'version', 'active')
		data_id = OperationsHelper.get_or_default(operation_config, 'data_id', None)
		target_column = OperationsHelper.get_or_default(operation_config, 'target_column', 'default-target')
		cache = OperationsHelper.get_or_default(operation_config, 'cache', True)

		# create directories required for caching if they do not exist
		if not os.path.exists(open_ml_datasets_local_copy):
			logger.info("Creating OpenML local copy directory at %s" % open_ml_datasets_local_copy)
			os.makedirs(open_ml_datasets_local_copy)

		logger.debug("Fetching dataset %s (name: %s version: %s) from OpenML..." % (data_id, name, version))
		data, target = OperationsOpenML.timeout(fetch_openml, (), {
			'name': name,
			'version': version,
			'data_id': data_id,
			'data_home': open_ml_datasets_local_copy,
			'target_column': target_column,
			'cache': cache,
			'return_X_y': True,
			'as_frame': True}, timeout_duration=15, default=(None, None))

		if data is None or target is None:
			logger.info("Fetching dataset %s (name: %s version: %s) from OpenML ran into timout" % (data_id, name, version))
			raise Exception("Fetching timeout")

		logger.info("Fetched dataset %s (name: %s version: %s) from OpenML" % (data_id, name, version))

		return [data, target]

	@staticmethod
	def timeout(func, args=(), kwargs=None, timeout_duration=1, default=None):
		if kwargs is None:
			kwargs = {}
		import signal

		class TimeoutError(Exception):
			pass

		def handler(signum, frame):
			raise TimeoutError()

		# set the timeout handler
		signal.signal(signal.SIGALRM, handler)
		signal.alarm(timeout_duration)
		try:
			result = func(*args, **kwargs)
		except TimeoutError as exc:
			result = default
		finally:
			signal.alarm(0)

		return result
