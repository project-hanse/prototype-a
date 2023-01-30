from src.helper.log_helper import LogHelper
from src.models.model_1_complementnb import Model1ComplementNB
from src.models.model_2_complementnb import Model2ComplementNB
from src.models.model_3_complementnb import Model3ComplementNB
from src.models.model_3_randomforest import Model3RandomForest
from src.models.model_base import ModelBase
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient


class ModelRegistry:
	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)
		self.models = {
			"model-1-complementnb": Model1ComplementNB(self.pipeline_client, self.dataset_client),
			"model-2-complementnb": Model2ComplementNB(self.pipeline_client, self.dataset_client),
			"model-3-complementnb": Model3ComplementNB(self.pipeline_client, self.dataset_client),
			"model-3-randomforest": Model3RandomForest(self.pipeline_client, self.dataset_client)
		}

	def get_model_by_name(self, name) -> ModelBase:
		self.logger.info("Getting model %s" % name)
		if name in self.models:
			return self.models[name]
		else:
			raise ModuleNotFoundError("Model %s not found" % name)

	def get_all_model_names(self):
		return list(self.models.keys())
