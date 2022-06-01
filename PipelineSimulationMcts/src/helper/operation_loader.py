import requests as requests

from src.helper.log_helper import LogHelper


class OperationLoader:
	def __init__(self, api_base_url: str, api_user: str, api_secret: str):
		self.api_base_url = api_base_url
		self.auth = (api_user, api_secret)
		self.operations_cache = None
		self.logger = LogHelper.get_logger(__name__)

	def load_operations(self) -> list:
		if self.operations_cache is None:
			self.logger.info("Loading operations from %s..." % self.api_base_url)
			response = requests.get(self.api_base_url + "/api/v1/operationTemplates", auth=self.auth)
			response.raise_for_status()
			self.operations_cache = response.json()
			self.logger.info("Loaded %d operations" % len(self.operations_cache))
		return self.operations_cache
