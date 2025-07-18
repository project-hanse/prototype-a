import os

from src.helper.log_helper import LogHelper

open_ml_task_offset_max = 5000
# Static configuration values for the application with defaults
max_dataset_inputs_per_operation = 4
# default to score operation
terminal_operation_ids = ['067c7cd4-87f6-43e2-a733-26e5c51ef875']
base_url_pipeline_service = "https://hanse.struempf.dev/api/pipeline"
base_url_learning_service = "https://hanse.struempf.dev/api/learning"
api_user = 'api_user'
api_secret = 'd49e3f0f-964e-4241-9693-31d2c80e5ecd'
pipelines_dir = 'pipelines'
reward_function_type = 'poly_peak'
verbose_level = 0
max_actions_per_pipeline = 30
partial_rewards_for_max_lookahead = 0.5
variance_reward_factor = 0.3
max_look_ahead_steps = 10
max_actions_per_state = 50
mcts_iteration_limit = 20
sleep_time_after_new_actions = 0.0
pipelines_per_batch = 25
expert_policy_model_name = 'model-3-complementnb'
expert_policy_probability = 0.75
cache_requests = True
target_action_count = 5


def init_config():
	logger = LogHelper.get_logger(__name__)
	logger.info("Initializing configuration")
	for key, value in globals().items():
		if key.startswith('get_') or key.startswith('__'):
			continue
		if type(value) is list or type(value) is tuple:
			globals()[key] = os.getenv(key.upper(), ','.join(value)).strip().replace(' ', '').split(',')
		elif type(value) is int:
			globals()[key] = int(os.getenv(key.upper(), value))
		elif type(value) is float:
			globals()[key] = float(os.getenv(key.upper(), value))
		elif type(value) is bool:
			globals()[key] = os.getenv(key.upper(), str(value)).strip().lower() == 'true'
		else:
			globals()[key] = os.getenv(key.upper(), value)
		if globals()[key] != value:
			logger.info("Overriding default value for key '{}' with value '{}'".format(key, globals()[key]))


def get_api_base_url_pipeline_service():
	return base_url_pipeline_service


def get_api_base_url_learning_service():
	return base_url_learning_service


def get_api_user():
	return api_user


def get_api_secret():
	return api_secret


def get_config(key: str):
	return globals()[key]


load_open_ml_operation = {
	"operationId": "9c876745-9d61-4b0b-a32a-2de523b44d0b",
	"operationName": "load_data_from_openml",
	"operationFullName": "openml.datasets.load",
	"inputTypes": [],
	"outputType": None,
	"outputTypes": [
		2,
		1
	],
	"framework": "openml",
	"frameworkVersion": "1.0.0",
	"sectionTitle": "OpenML",
	"title": "openml.datasets.load",
	"description": "Fetch dataset from OpenML by name or dataset id.",
	"signature": "load_data_from_openml",
	"signatureName": None,
	"returnsDescription": None,
	"sourceUrl": "https://scikit-learn.org/stable/modules/generated/sklearn.datasets.fetch_openml.html",
	"defaultConfig": {
		"name": None,
		"version": "active",
		"data_id": None,
		"data_home": None,
		"target_column": "default-target",
		"cache": "true",
		"timeout": "30",
	},
	"returns": "Dataframes"
}
