from abc import abstractmethod

from sklearn.model_selection._search import BaseSearchCV

from src.helper.log_helper import LogHelper
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class TrainerModelBase:
	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)

	@abstractmethod
	def get_model_pipeline(self) -> BaseSearchCV:
		raise NotImplementedError()

	@abstractmethod
	def get_data(self, cache=True) -> (list, list):
		raise NotImplementedError()

	def _load_data(self, cache=True) -> (list, list):
		self.logger.debug("Loading data for %s", __name__)
		tuples = self.pipeline_client.get_pipeline_tuples(cache=cache)
		tuples_with_metadata = []
		loaded_input_metadata_cnt = 0
		total_input_cnt = 0
		for op_tuple in tuples:
			op_tuple['targetInputMetadata'] = []
			for op_input in op_tuple['targetInputs']:
				metadata = {}
				total_input_cnt += 1
				try:
					metadata['dataset_type'] = op_input['type']
					metadata_rmt = self.dataset_client.get_metadata(op_input['key'], cache=cache)
					metadata.update(metadata_rmt)
					if 'type' in metadata:
						metadata['storage_type'] = metadata['type']
						del metadata['type']
					op_tuple['targetInputMetadata'].append(metadata)
					loaded_input_metadata_cnt += 1
				except Exception as e:
					self.logger.warning("Failed to load %s (%s)" % (op_input['key'], str(e)))
					op_tuple['targetInputMetadata'].append(metadata)
					continue
			tuples_with_metadata.append(op_tuple)
		self.logger.info("Got %d tuples with metadata for %d/%d input datasets" % (
			len(tuples), loaded_input_metadata_cnt, total_input_cnt))
		feat, lab = self._tuples_preprocessing(tuples_with_metadata)
		return feat, lab

	# Preprocess tuples
	def _flatten_dict(self, d, parent_key='', sep='_'):
		items = []
		for k, v in d.items():
			new_key = parent_key + sep + k if parent_key else k
			if isinstance(v, dict):
				items.extend(self._flatten_dict(v, new_key, sep=sep).items())
			elif isinstance(v, list):
				# Ignore lists for now
				pass
			else:
				items.append((new_key, v))
		return dict(items)

	def _tuples_preprocessing(self, data: []) -> ([{}], [{}]):
		feat = []
		lab = []
		for element in data:
			if "targetOperationIdentifier" not in element or "targetInputMetadata" not in element:
				self.logger.warning("Missing essential keys in element: %s", element)
				continue

			lab.append(element["targetOperationIdentifier"])
			input_dataset_count = 0
			feat_vec = {}
			for input_meta in element['targetInputMetadata']:
				for key, val in self._flatten_dict(input_meta, parent_key='input_' + str(input_dataset_count), sep='_').items():
					feat_vec[key] = val
				input_dataset_count += 1
			feat_vec["feat_pred_id"] = element['predecessorOperationIdentifier']
			feat_vec['feat_pred_input_count'] = len(element['targetInputs'])
			if feat_vec is {}:
				self.logger.warning("Empty feature vector for element: %s", element)
			feat.append(feat_vec)

		return feat, lab

	def _select_features(self, feat: [{}], keys: [str]) -> [{}]:
		"""
		Selects features (by key) from the feature values array.
		If a value is not found, it is set to None.
		"""
		if keys is None or len(keys) == 0:
			self.logger.debug("No feature keys provided")
			return feat
		self.logger.info("Selecting features with keys %s", keys)
		new_feat = []
		for element in feat:
			new_element = {}
			for key, val in element.items():
				if key in keys:
					new_element[key] = val
				else:
					new_element[key] = '-1'
			new_feat.append(new_element)
		return new_feat
