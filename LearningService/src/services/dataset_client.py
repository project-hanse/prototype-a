import requests

from src.helper.log_helper import LogHelper


class DatasetClient:
	def __init__(self, host: str, port: int):
		self.host = host
		self.port = port
		self.logger = LogHelper.get_logger(__name__)
		self.cache = {}

	def get_base_url(self) -> str:
		return f"http://{self.host}:{self.port}"

	def get_metadata(self, key: str, dataset_type: str = '', cache: bool = True) -> dict:
		if cache:
			if key in self.cache:
				return self.cache[key]
		self.logger.debug("Loading metadata for key %s", key)
		response = requests.get(self.get_base_url() + "/api/metadata/key/%s?version=compact" % key)
		if response.status_code != 200:
			if response.status_code == 404:
				self.logger.debug("Metadata not found for key %s of datatype %s" % (key, dataset_type))
			else:
				self.logger.warning(
					"Failed to load metadata for key %s of type %s (status: %i)" % (key, dataset_type, response.status_code))
			return {}
		metadata = response.json()
		self.cache[key] = metadata
		return metadata
