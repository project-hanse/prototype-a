import requests

from src.helper.log_helper import LogHelper


class PipelineClient:
	def __init__(self, host: str, port: int):
		self.host = host
		self.port = port
		self.logger = LogHelper.get_logger(__name__)
		self.cache = {}

	def get_base_url(self) -> str:
		return f"http://{self.host}:{self.port}/api/v1/pipelines"

	def get_pipeline_tuples(self, cache: bool = True) -> list:
		self.logger.debug("Getting pipeline tuples")
		if cache:
			self.logger.debug("Getting pipeline tuples from cache")
			if "tuples" in self.cache:
				self.logger.info("Returning %d pipeline tuples from cache" % len(self.cache["tuples"]))
				return self.cache["tuples"]

		self.logger.debug("Loading pipeline tuples from server")
		response = requests.get(self.get_base_url() + "/tuples")
		try:
			response.raise_for_status()
		except requests.exceptions.HTTPError as e:
			self.logger.error("Failed to get pipeline tuples: %s" % e)
			return []
		tuples = response.json()
		self.cache["tuples"] = tuples
		self.logger.info("Loaded %d pipeline tuples from server" % len(tuples))
		return tuples
