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

	def get_trainer_by_model_name(self, name):
		self.logger.info("Getting trainer for model %s" % name)
		if name == "model-1-complementnb":
			return TrainerModel1ComplementNB(self.pipeline_client, self.dataset_client)
		elif name == "model-2-complementnb":
			return TrainerModel2ComplementNB(self.pipeline_client, self.dataset_client)
		elif name == "model-3-complementnb":
			return TrainerModel3ComplementNB(self.pipeline_client, self.dataset_client)
		elif name == "model-3-randomforest":
			return TrainerModel3RandomForest(self.pipeline_client, self.dataset_client)
		else:
			raise ModuleNotFoundError("Model %s not found" % name)
