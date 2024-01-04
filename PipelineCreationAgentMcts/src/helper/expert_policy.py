import logging
import random

import requests

from src.config.config import get_api_secret, get_api_user, \
	get_api_base_url_learning_service, get_config
from src.model.pipeline_state import PipelineBuildingState


def model_policy(state: PipelineBuildingState):
	model_name = get_config('expert_policy_model_name')
	while not state.is_terminal():
		action = None
		try:
			if state.producing_operation is None or random.random() < get_config('expert_policy_probability'):
				action = random.choice(state.get_possible_actions())
			else:
				suggested_op_identifiers = load_predicted_operation_identifier(state.producing_operation, model_name)
				for possible_action in state.get_possible_actions():
					random.shuffle(suggested_op_identifiers)
					for suggested_op_identifier in suggested_op_identifiers:
						if get_operation_identifier(possible_action.operation) == suggested_op_identifier:
							action = possible_action
							break
				if action is None:
					action = random.choice(state.get_possible_actions())

		except IndexError:
			raise Exception("Non-terminal state has no possible actions: " + str(state))
		state = state.take_action(action)
	return state.get_reward()


def load_predicted_operation_identifier(operation: dict, model_name: str) -> [str]:
	payload = {
		'feat_pred_count': 1,
		'feat_pred_id': get_operation_identifier(operation)
	}
	for i, input_datatype in zip(range(len(operation['outputTypes'])), operation['outputTypes']):
		payload["input_%d_dataset_type" % i] = input_datatype
	request = requests.post(
		url=get_api_base_url_learning_service() + "/api/predict/%s" % model_name,
		auth=(get_api_user(), get_api_secret()),
		json=payload)
	if request.status_code != 200:
		logging.warning("Failed to load predicted operation identifier: (%d) %s" % (request.status_code, request.text))
		return []
	response = request.json()
	return response


def get_operation_identifier(operation: dict):
	return "%s-%s" % (operation['operationId'], operation['operationName'])
