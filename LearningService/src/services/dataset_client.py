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

	def get_metadata(self, key: str, cache: bool = True) -> dict:
		if cache:
			if key in self.cache:
				return self.cache[key]
		self.logger.debug("Loading metadata for key %s", key)
		response = requests.get(self.get_base_url() + "/api/metadata/key/%s?version=compact" % key)
		response.raise_for_status()
		metadata = response.json()
		self.cache[key] = metadata
		return metadata
