from src.config.config import get_api_base_url_pipeline_service, get_api_secret, get_api_user
from src.helper.operation_loader import OperationLoader


class HelperFactory:
	def __init__(self):
		self.operation_loader = None

	def get_operation_loader(self):
		if self.operation_loader is None:
			self.operation_loader = OperationLoader(api_base_url=get_api_base_url_pipeline_service(),
																							api_user=get_api_user(),
																							api_secret=get_api_secret())
		return self.operation_loader
