import glob
import json
import os
import shutil

import numpy as np
from sklearn.feature_extraction import DictVectorizer
from spektral.data import Dataset, Graph

from src.helper.log_helper import LogHelper
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class GraphDataset(Dataset):
	"""
	A dataset is used by spektral to load the data and to create the batches.
	In this case the data is loaded from the pipeline service and dataset service and then converted to a graph.
	"""

	pipelines_subdir = 'pipelines'
	top_sort_subdir = 'sort'
	feature_encoder = None
	label_encoder = None

	min_nodes_in_graph = 2
	max_nodes_in_graph = 64 * 4

	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient, reload: bool = False, **kwargs):
		"""
		Constructor for a new dataset.
		:param reload: if true the current local copy of the dataset will be deleted and redownloaded.
		"""
		self.logger = LogHelper.get_logger(__name__)
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		if reload:
			shutil.rmtree(self.path)

		super().__init__(**kwargs)

	def download(self) -> None:
		"""
		Loads successfully executed pipelines from the prototype deployment and stores them as json files in self.path.
		"""
		self.logger.info("Downloading datasets...")

		if not os.path.exists(os.path.join(self.path, self.pipelines_subdir)):
			os.makedirs(os.path.join(self.path, self.pipelines_subdir))

		if not os.path.exists(os.path.join(self.path, self.top_sort_subdir)):
			os.makedirs(os.path.join(self.path, self.top_sort_subdir))

		pipeline_dtos = self.pipeline_client.get_pipeline_dtos()

		# TODO: do this in database
		# filter pipeline_dtos where successfullyExecutable is false
		pipeline_dtos = list(filter(lambda x: (x['successfullyExecutable']), pipeline_dtos))

		counter_structure = 0
		counter_sorted = 0
		for pipeline_dto in pipeline_dtos:
			pipeline_id = pipeline_dto['id']
			pipeline_structure = self.pipeline_client.get_pipeline_structure(pipeline_id)
			if pipeline_structure is None:
				self.logger.warning("Pipeline %s has no structure", pipeline_id)
				continue
			with open(os.path.join(self.path, self.pipelines_subdir, str(pipeline_id) + '.json'), 'w') as file:
				file.write(pipeline_structure)
				counter_structure += 1
			pipeline_sorted = self.pipeline_client.get_pipeline_sorted(pipeline_id)
			if pipeline_sorted is None:
				self.logger.warning("Pipeline %s has no topological sort", pipeline_id)
				continue
			with open(os.path.join(self.path, self.top_sort_subdir, str(pipeline_id) + '.json'), 'w') as file:
				file.write(pipeline_sorted)
				counter_sorted += 1
		self.logger.info("Downloaded %s pipeline structures and %s topological sorts", counter_structure, counter_sorted)

	def _group_by_level(self, nodes: []) -> [[]]:
		"""
		Groups nodes into an array of arrays where all nodes with the same level value are in one group.
		"""
		self.logger.debug("Grouping nodes by level...")
		groups_dict = {}
		for node in nodes:
			if node['level'] not in groups_dict:
				groups_dict[str(node['level'])] = []
			groups_dict[str(node['level'])].append(node)
		groups_array = []
		for key, value in groups_dict.items():
			groups_array.append(value)
		return groups_array

	def _build_node_encodings_label(self, nodes: []) -> ({}, int):
		"""
		Generates encodings (used as labels of the model) for all nodes passed and stores them in a dictionary.

		The  dictionary has operation identifiers as keys and an array of labels for each node as values.
	 	returns: a label map and the number of dimensions used to encode the node features.
		"""
		self.logger.debug("Building node label map...")
		self.label_encoder = DictVectorizer(sparse=False)
		ids = list()
		labels = list()
		for node in nodes:
			ids.append(node['operationId'])
			labels.append({
				'opId': node['operationIdentifier']
			})
		encoded_labels = self.label_encoder.fit_transform(labels)
		label_map = {}
		for k, v in zip(ids, encoded_labels):
			label_map[k] = np.array(v)
		return label_map, len(encoded_labels[0])

	def _build_node_encodings_features(self, nodes: []) -> int:
		"""
		Generates encodings (used as inputs of the model) for all nodes and stores them in the feature_encoder.
		"""
		self.feature_encoder = DictVectorizer(sparse=False)
		features = []
		for node in nodes:
			features.append(self._node_to_feature_dict(node))
		encoded_features = self.feature_encoder.fit_transform(features)
		return len(encoded_features[0])

	def _node_to_feature_dict(self, node):
		"""
		Converts a node to a dictionary of features.
		"""
		return {
			'opTempId': node['operationTemplateId'],
			'opName': node['operationName'],
			'opGlobId': node['operationIdentifier'],
		}

	def _encode_node_to_feature(self, node):
		"""
		Encodes a node to a feature vector.
		"""
		return self.feature_encoder.transform([self._node_to_feature_dict(node)])

	def decode_labels(self, l):
		return self.label_encoder.inverse_transform(l)

	def read(self) -> list[Graph]:
		self.logger.debug("Reading pipelines from %s and converting to graphs...", self.path)

		# encoding features and labels for all nodes
		all_nodes = []
		for filepath in glob.iglob(os.path.join(self.path, self.pipelines_subdir, '*.json'), recursive=False):
			with open(filepath) as pipeline_file:
				pipeline = json.loads(pipeline_file.read())
				all_nodes.extend(pipeline['nodes'])

		dims_feat = self._build_node_encodings_features(all_nodes)
		node_encodings_labels, dims_lab = self._build_node_encodings_label(all_nodes)

		# converting pipelines to graphs
		counter_pipelines = 0
		output = []
		for filepath in glob.iglob(os.path.join(self.path, self.pipelines_subdir, '*.json'), recursive=False):
			self.logger.debug("Reading pipeline from %s...", filepath)
			prev_output_count = len(output)
			with open(filepath) as pipeline_file:
				pipeline = json.loads(pipeline_file.read())

			if pipeline is None or pipeline['pipelineId'] is None:
				self.logger.warning("Skipping empty pipeline")
				continue

			pipeline_id = pipeline['pipelineId']

			# Skip pipelines that are too large or too small
			if len(pipeline['nodes']) > self.max_nodes_in_graph:
				self.logger.info("Skipping pipeline (%s) with %i nodes since it exceeds maximum number of nodes %i",
												 pipeline_id, len(pipeline['nodes']), self.max_nodes_in_graph)
				continue
			if len(pipeline['nodes']) < self.min_nodes_in_graph:
				self.logger.info("Skipping pipeline (%s) with %i nodes since it falls short of minimum number of nodes %i",
												 pipeline_id, len(pipeline['nodes']), self.min_nodes_in_graph)
				continue

			self.logger.debug("Processing pipeline %s...", pipeline_id)
			counter_pipelines += 1

			with open(os.path.join(self.path, self.top_sort_subdir, pipeline_id + ".json")) as pipeline_sort:
				top_sort = json.loads(pipeline_sort.read())

			node_groups = self._group_by_level(top_sort)

			# Convert pipeline to graph representation (adjacency matrix, node features, edge features)
			nodes_count = len(pipeline['nodes'])
			edges_count = len(pipeline['edges'])
			# node features
			x = np.zeros(shape=(nodes_count, dims_feat))
			# adjacency matrix
			a = np.zeros(shape=(nodes_count, nodes_count))
			# edge features
			e = np.zeros(shape=(nodes_count, nodes_count, 1))

			node_id_index_mapping = dict()
			for i in range(1, len(node_groups)):
				current_group = node_groups[i - 1]
				next_group = node_groups[i]

				for node in current_group:
					node_id_index_mapping[node['operationId']] = len(node_id_index_mapping)
					x[node_id_index_mapping[node['operationId']]] = self._encode_node_to_feature(node)

				for edge in pipeline['edges']:
					if edge['from'] in node_id_index_mapping and edge['to'] in node_id_index_mapping:
						a[node_id_index_mapping[edge['from']]][node_id_index_mapping[edge['to']]] = 1

				# generate a graph for each node in the next group
				for node in next_group:
					# labels
					y = node_encodings_labels[node['operationId']]
					output.append(Graph(x=x, a=a, e=None, y=y))
			self.logger.info("Converted pipeline %s to %i graph(s)", pipeline_id, len(output) - prev_output_count)

		self.logger.info("Generated %i graphs from %i pipelines", len(output), counter_pipelines)
		return output
