import requests

from src.helper.log_helper import LogHelper


class PipelineClient:
	def __init__(self, host: str, port: int):
		self.host = host
		self.port = port
		self.logger = LogHelper.get_logger(__name__)
		self.cache = {}

	def get_url(self, path: str, query_params: dict = None) -> str:
		url = f"http://{self.host}:{self.port}/{path}"
		if query_params:
			query_string = "&".join([f"{k}={v}" for k, v in query_params.items()])
			url = f"{url}?{query_string}"
		return url

	def get_pipeline_tuples(self, cache: bool = True) -> list:
		self.logger.debug("Getting pipeline tuples")
		if cache:
			self.logger.debug("Getting pipeline tuples from cache")
			if "tuples" in self.cache:
				self.logger.info("Returning %i pipeline tuples from cache" % len(self.cache["tuples"]))
				return self.cache["tuples"]

		self.logger.debug("Loading pipeline tuples from server")
		response = requests.get(self.get_url("api/v1/pipelines/tuples"))
		try:
			response.raise_for_status()
		except requests.exceptions.HTTPError as e:
			self.logger.error("Failed to get pipeline tuples: %s" % e)
			return []
		tuples = response.json()
		self.cache["tuples"] = tuples
		self.logger.info("Loaded %i pipeline tuples from server" % len(tuples))
		return tuples

	def get_pipeline_dtos(self) -> list:
		self.logger.debug("Getting pipeline dtos")
		pipeline_dtos = []
		page = 0
		while True:
			response = requests.get(self.get_url('api/v1/pipelines', {"page": page, "pageSize": 25}))
			response.raise_for_status()
			response_json = response.json()
			pipeline_dtos.extend(response_json['items'])
			if 'totalItems' in response_json and len(pipeline_dtos) < response_json['totalItems']:
				page += 1
				self.logger.debug(
					"Loaded %i/%i pipelines, loading next page" % (len(pipeline_dtos), response_json['totalItems']))
			else:
				break
		self.logger.info("Loaded %i pipelines" % len(pipeline_dtos))
		return pipeline_dtos

	def get_pipeline_structure(self, pipeline_id: str) -> str:
		"""
		Loads a json string containing the structure of a pipeline.
		"""
		self.logger.debug("Loading pipeline structure for pipeline %s" % pipeline_id)
		response = requests.get(self.get_url("api/v1/pipelines/%s/vis" % pipeline_id))
		try:
			response.raise_for_status()
		except requests.exceptions.HTTPError as e:
			self.logger.error("Failed to get pipeline structure: %s" % e)
			return None
		return response.text

	def get_pipeline_sorted(self, pipeline_id: str) -> str:
		"""
		Loads a json string containing a topological ordering of a pipeline.
		"""
		self.logger.debug("Loading pipeline sorted for pipeline %s" % pipeline_id)
		response = requests.get(self.get_url("api/v1/pipelines/%s/sort/topological" % pipeline_id))
		try:
			response.raise_for_status()
		except requests.exceptions.HTTPError as e:
			self.logger.error("Failed to get pipeline sorted: %s" % e)
			return None
		return response.text
