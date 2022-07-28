import logging
import os

import pandas as pd
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
		data_home = OperationsHelper.get_or_default(operation_config, 'data_home', None)
		target_column = OperationsHelper.get_or_default(operation_config, 'target_column', 'default-target')
		cache = OperationsHelper.get_or_default(operation_config, 'cache', True)

		dataset_dir = os.path.join(open_ml_datasets_local_copy, str(data_id), str(version))
		# create directories required for caching if they do not exist
		if not os.path.exists(open_ml_datasets_local_copy):
			logger.info("Creating OpenML local copy directory at %s" % open_ml_datasets_local_copy)
			os.makedirs(open_ml_datasets_local_copy)
		if not os.path.exists(dataset_dir):
			logger.info("Creating OpenML dataset local copy directory %s" % dataset_dir)
			os.makedirs(dataset_dir)

		if not cache or not os.listdir(dataset_dir):
			logger.info("Fetching dataset %s (version: %s) from OpenML..." % (data_id, version))
			data, target = fetch_openml(name=name, version=version, data_id=data_id, data_home=data_home,
																	target_column=target_column, cache=cache, return_X_y=True, as_frame=True)
			logger.debug("Fetched dataset %s (version: %s) from OpenML" % (data_id, version))
			# save dataset to local copy
			logger.info("Saving dataset %s (version: %s) to local copy at %s" % (data_id, version, dataset_dir))
			data.to_json(os.path.join(dataset_dir, "data.json"))
			target.to_json(os.path.join(dataset_dir, "target.json"))
		else:
			logger.info("Loading dataset %s (version: %s) from local copy" % (data_id, version))
			data = pd.read_json(os.path.join(dataset_dir, "data.json"), typ='frame')
			target = pd.read_json(os.path.join(dataset_dir, "target.json"), typ='series')
			logger.debug("Loaded dataset %s (version: %s) from local copy" % (data_id, version))

		return [data, target]
