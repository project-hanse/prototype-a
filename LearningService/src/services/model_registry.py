from src.helper.log_helper import LogHelper
from src.models._archive.model_1_complementnb import Model1ComplementNB
from src.models._archive.model_2_complementnb import Model2ComplementNB
from src.models._archive.model_3_complementnb import Model3ComplementNB
from src.models._archive.model_4_gnn import Model4GraphNeuralNetwork
from src.models.model_1_mlp import Model1MLP
from src.models.model_1_naivebayes import Model1NaiveBayes
from src.models.model_1_randomforest import Model1RandomForest
from src.models.model_2_mlp import Model2MLP
from src.models.model_2_naivebayes import Model2NaiveBayes
from src.models.model_2_randomforest import Model2RandomForest
from src.models.model_3_mlp import Model3MLP
from src.models.model_3_naivebayes import Model3NaiveBayes
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
			"model-1-naive-bayes": Model1NaiveBayes(self.pipeline_client, self.dataset_client),
			"model-2-naive-bayes": Model2NaiveBayes(self.pipeline_client, self.dataset_client),
			"model-3-naive-bayes": Model3NaiveBayes(self.pipeline_client, self.dataset_client),
			"model-1-random-forest": Model1RandomForest(self.pipeline_client, self.dataset_client),
			"model-2-random-forest": Model2RandomForest(self.pipeline_client, self.dataset_client),
			"model-3-random-forest": Model3RandomForest(self.pipeline_client, self.dataset_client),
			"model-1-multi-layer-perceptron": Model1MLP(self.pipeline_client, self.dataset_client),
			"model-2-multi-layer-perceptron": Model2MLP(self.pipeline_client, self.dataset_client),
			"model-3-multi-layer-perceptron": Model3MLP(self.pipeline_client, self.dataset_client),

			# Legacy models
			"model-1-complementnb": Model1ComplementNB(self.pipeline_client, self.dataset_client),
			"model-2-complementnb": Model2ComplementNB(self.pipeline_client, self.dataset_client),
			"model-3-complementnb": Model3ComplementNB(self.pipeline_client, self.dataset_client),
			"model-4-gnn-simple": Model4GraphNeuralNetwork(self.pipeline_client, self.dataset_client),
		}

	def get_model_by_name(self, name) -> ModelBase:
		self.logger.info("Getting model %s" % name)
		if name in self.models:
			return self.models[name]
		else:
			raise ModuleNotFoundError("Model %s not found" % name)

	def get_all_model_names(self):
		return list(self.models.keys())
