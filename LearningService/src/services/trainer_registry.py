from src.helper.log_helper import LogHelper
from src.services.dataset_client import DatasetClient
from src.services.pipeline_client import PipelineClient
from src.trainers.model_1_complementnb import TrainerModel1ComplementNB
from src.trainers.model_2_complementnb import TrainerModel2ComplementNB
from src.trainers.model_3_complementnb import TrainerModel3ComplementNB
from src.trainers.model_3_randomforest import TrainerModel3RandomForest


class TrainerRegistry:
	def __init__(self, pipeline_client: PipelineClient, dataset_client: DatasetClient):
		self.dataset_client = dataset_client
		self.pipeline_client = pipeline_client
		self.logger = LogHelper.get_logger(__name__)
		self.trainers = {
			"model-1-complementnb": TrainerModel1ComplementNB(self.pipeline_client, self.dataset_client),
			"model-2-complementnb": TrainerModel2ComplementNB(self.pipeline_client, self.dataset_client),
			"model-3-complementnb": TrainerModel3ComplementNB(self.pipeline_client, self.dataset_client),
			"model-3-randomforest": TrainerModel3RandomForest(self.pipeline_client, self.dataset_client)
		}

	def get_trainer_by_model_name(self, name):
		self.logger.info("Getting trainer for model %s" % name)
		if name in self.trainers:
			return self.trainers[name]
		else:
			raise ModuleNotFoundError("Model %s not found" % name)

	def get_all_trainer_names(self):
		return list(self.trainers.keys())
