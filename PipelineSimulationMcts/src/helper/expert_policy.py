import random

import requests

from src.config.config import get_api_secret, get_api_user, \
	get_api_base_url_learning_service, get_config
from src.model.pipeline_state import PipelineBuildingState

"""
{
  "input_0_dataset_type": 6,
  "feat_pred_count": 1,
  "feat_pred_id": "f3fc1084-4c1a-495d-846c-013f6f37985c-label_encoder"
}
"""


def model3_policy(state: PipelineBuildingState):
	while not state.is_terminal():
		action = None
		try:
			if state.producing_operation is None or random.random() < get_config('expert_policy_probability'):
				action = random.choice(state.get_possible_actions())
			else:
				payload = {
					'feat_pred_count': 1,
					'feat_pred_id': get_operation_identifier(state.producing_operation)
				}
				for i, input_datatype in zip(range(len(state.producing_operation['outputTypes'])),
																		 state.producing_operation['outputTypes']):
					payload["input_%d_dataset_type" % i] = input_datatype
				request = requests.post(
					url=get_api_base_url_learning_service() + "/api/predict/" + get_config('expert_policy_model_name'),
					auth=(get_api_user(), get_api_secret()),
					json=payload)
				request.raise_for_status()
				response = request.json()
				suggested_op_identifier = response[0]
				for possible_action in state.get_possible_actions():
					if get_operation_identifier(possible_action.operation) == suggested_op_identifier:
						action = possible_action
						break
				if action is None:
					action = random.choice(state.get_possible_actions())

		except IndexError:
			raise Exception("Non-terminal state has no possible actions: " + str(state))
		state = state.take_action(action)
	return state.get_reward()


def get_operation_identifier(operation: dict):
	return "%s-%s" % (operation['operationId'], operation['operationName'])
