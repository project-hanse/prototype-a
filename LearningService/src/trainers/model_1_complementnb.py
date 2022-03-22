from sklearn.dummy import DummyClassifier
from sklearn.pipeline import Pipeline, make_pipeline

from src.helper.log_helper import LogHelper
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class TrainerModel1ComplementNB:
	feature_names = ['type', 'input_0_dataType', 'model_type']

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)

	def get_model_pipeline(self) -> Pipeline:
		self.logger.debug("Creating model pipeline for %s", __name__)
		ppl = make_pipeline(DummyClassifier())
		return ppl

	def get_data(self) -> (list, list):
		self.logger.debug("Getting data for %s", __name__)
		tuples = self.pipeline_client.get_pipeline_tuples()
		tuples_with_metadata = []
		for op_tuple in tuples:
			op_tuple['targetInputMetadata'] = []
			for op_input in op_tuple['targetInputs']:
				try:
					metadata = self.dataset_client.get_metadata(op_input['key'])
					metadata['dataType'] = op_input['type']
					op_tuple['targetInputMetadata'].append(metadata)
				except Exception as e:
					print("Failed to load %s (%s)" % (op_input['key'], str(e)))
					op_tuple['targetInputMetadata'].append({})
					continue
			tuples_with_metadata.append(op_tuple)
		self.logger.info("Got %d/%d tuples with metadata" % (len(tuples_with_metadata), len(tuples)))
		feat, lab = self._tuples_preprocessing(tuples_with_metadata)
		feat = self._whitelist_features(feat, self.feature_names)
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

	def _tuples_preprocessing(self, data: []) -> []:
		feat = []
		lab = []
		for element in data:
			if "targetOperationIdentifier" not in element or "targetInputMetadata" not in element:
				self.logger.warning("Missing essential keys in element: %s", element)
				continue

			lab.append(element['targetOperationIdentifier'])
			c = 0
			vector_element_count = 0
			feat_vec = {}
			for input_meta in element['targetInputMetadata']:
				for key, val in self._flatten_dict(input_meta, parent_key='input_' + str(c), sep='_').items():
					feat_vec[key] = val
					vector_element_count += 1
				c += 1
			feat.append(feat_vec)

		return feat, lab

	def _whitelist_features(self, feat: [{}], key_contains: [str]) -> [{}]:
		new_feat = []
		for element in feat:
			new_element = {}
			for key, val in element.items():
				for contains in key_contains:
					if contains in key:
						new_element[key] = val
			new_feat.append(new_element)
		return new_feat
