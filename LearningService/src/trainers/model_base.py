from abc import abstractmethod

from sklearn.pipeline import Pipeline

from src.helper.log_helper import LogHelper
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class TrainerModelBase:
	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)

	@abstractmethod
	def get_model_pipeline(self) -> Pipeline:
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
					metadata['dataType'] = op_input['type']
					metadata = self.dataset_client.get_metadata(op_input['key'], cache=cache)
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

	def _whitelist_features(self, feat: [{}], key_contains: [str]) -> [{}]:
		if key_contains is None or len(key_contains) == 0:
			self.logger.debug("No whitelist keys provided")
			return feat
		new_feat = []
		for element in feat:
			new_element = {}
			for key, val in element.items():
				for contains in key_contains:
					if contains in key:
						new_element[key] = val
			new_feat.append(new_element)
		return new_feat
