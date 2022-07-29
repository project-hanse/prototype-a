import logging
import multiprocessing
import os
import time

from src.constants.dirs import open_ml_datasets_local_copy
from src.helper.fetch_helper import custom_fetch_openml
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
		timeout = OperationsHelper.get_or_default(operation_config, 'timeout', 25)

		# create directories required for caching if they do not exist
		if not os.path.exists(open_ml_datasets_local_copy):
			logger.info("Creating OpenML local copy directory at %s" % open_ml_datasets_local_copy)
			os.makedirs(open_ml_datasets_local_copy)

		logger.debug("Fetching dataset %s (name: %s version: %s) from OpenML..." % (data_id, name, version))

		# execute fetch_openml_or_timeout in new thread and await result
		loading_thread = multiprocessing.Process(target=custom_fetch_openml,
																						 args=(name, version, data_id, open_ml_datasets_local_copy, target_column,
																									 cache, True, True))
		loading_thread.start()
		wait_counter = 0
		while loading_thread.is_alive():
			time.sleep(1)
			wait_counter += 1
			if wait_counter > timeout:
				logger.warning(
					"Waiting for dataset %s (name: %s version: %s) to finish took more than %i seconds. Aborting..." % (
						data_id, name, version, timeout))
				loading_thread.kill()
				raise Exception(
					"Fetching dataset %s (name: %s version: %s) from OpenML took more than %i seconds. Aborting..." % (
						data_id, name, version, timeout))
			if wait_counter % 3 == 0:
				logger.debug("Waiting for dataset %s (name: %s version: %s) to finish..." % (data_id, name, version))

		# dirty hack to avoid passing loaded data from loading_thread (loads data from cache)
		data, target = custom_fetch_openml(name, version, data_id, open_ml_datasets_local_copy, target_column,
																			 cache, True, True)

		logger.info("Fetched dataset %s (name: %s version: %s) from OpenML" % (data_id, name, version))

		return [data, target]
