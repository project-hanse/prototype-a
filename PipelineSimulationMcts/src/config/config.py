import os

# Static configuration values for the application with defaults
max_dataset_inputs_per_operation = 4
# default to score operation
terminal_operation_ids = ['067c7cd4-87f6-43e2-a733-26e5c51ef875']
base_url_pipeline_service = "https://hanse.allteams.at/api/pipeline"
base_url_learning_service = "https://hanse.allteams.at/api/learning"
api_user = 'api_user'
api_secret = 'd49e3f0f-964e-4241-9693-31d2c80e5ecd'
verbose_level = 0
max_actions_per_pipeline = 30
partial_rewards_for_max_lookahead = 0.5
variance_reward_factor = 0.3
max_look_ahead_steps = 10
mcts_iteration_limit = 20


def get_open_ml_task_ids() -> [str]:
	return os.getenv("OPEN_ML_TASK_IDS", '31').strip().replace(' ', '').split(',')


def get_terminal_operation_ids() -> [str]:
	return os.getenv("TERMINAL_OPERATION_IDS", '067c7cd4-87f6-43e2-a733-26e5c51ef875').strip().replace(' ', '').split(',')


def get_api_base_url_pipeline_service():
	return base_url_pipeline_service


def get_api_base_url_learning_service():
	return base_url_learning_service


def get_api_user():
	return api_user


def get_api_secret():
	return api_secret


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
		"cache": "true"
	},
	"returns": "Dataframes"
}
