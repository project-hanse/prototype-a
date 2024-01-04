import json
import math
import os
import random
import time
import uuid
from datetime import timedelta

import openml as openml
import requests_cache
from mcts.searcher.mcts import MCTS
from openml import OpenMLTask

from src.config.config import get_config, init_config
from src.helper.expert_policy import model_policy
from src.helper.helper_factory import HelperFactory
from src.helper.log_helper import LogHelper
from src.helper.serializer import TMCSerializer
from src.model.pipeline_state import PipelineBuildingState

logger = LogHelper.get_logger('main')


def get_initial_state(task_id: int) -> (OpenMLTask, PipelineBuildingState):
	logger.info("Loading task %d details from OpenML..." % task_id)
	task = openml.tasks.get_task(task_id)
	if task.task_type_id == openml.tasks.TaskType.SUPERVISED_CLASSIFICATION:
		state = PipelineBuildingState(helper_factory=HelperFactory(),
																	available_datasets=[{'type': 2, 'key': uuid.uuid4()},
																											{'type': 1, 'key': uuid.uuid4()}],
																	producing_operation=get_config('load_open_ml_operation'),
																	max_look_ahead=get_config('max_look_ahead_steps'),
																	max_actions_per_state=get_config('max_actions_per_state'),
																	verbose=get_config('verbose_level'))
		state.producing_operation['defaultConfig']['data_id'] = task.dataset_id
		return task, state

	raise Exception('Task type not supported')


def save_pipeline(ppln):
	os.makedirs(get_config('pipelines_dir'), exist_ok=True)
	with open(os.path.join(get_config('pipelines_dir'), 'pipeline-%s.json' % math.floor(time.time())), 'w') as f:
		logger.info('Saving pipeline %d (batch %d) to %s' % (ppln['pipeline_number'], ppln['batch_number'], f.name))
		json.dump(ppln, f, cls=TMCSerializer)


if __name__ == '__main__':
	init_config()
	logger.info("\n**********")
	logger.info(
		"Starting pipeline creation agent (expert_policy_model: '%s', expert_policy_probability: %.2f, reward_function: '%s')..." % (
			get_config('expert_policy_model_name'),
			get_config('expert_policy_probability'),
			get_config('reward_function_type')))

	requests_cache.install_cache('mcts_cache',
															 expire_after=timedelta(minutes=5),
															 cache_control=False,
															 allowable_methods=['GET', 'POST'])

	offset = random.randint(0, get_config('open_ml_task_offset_max'))
	logger.info("Loading possible tasks from OpenML...")
	task_options = [31, 10101, 9914, 145804, 146065, 146064, 125923, 3913, 3, 3917]
	logger.info("Loaded %d tasks from OpenML" % len(task_options))
	searcher = MCTS(iterationLimit=get_config('mcts_iteration_limit'), rolloutPolicy=model_policy)
	batch_number = random.randint(0, 10000)
	for i in range(get_config('pipelines_per_batch')):
		task_id = random.choice(task_options)
		logger.info('Simulating pipeline %s for task %i' % (i, task_id))
		task, currentState = get_initial_state(task_id)
		pipeline = {
			'actions': [{
				'operation': currentState.producing_operation,
				'input_datasets': [],
				'output_datasets': currentState.available_datasets
			}],
			'pipeline_id': uuid.uuid4(),
			'started_at': math.floor(time.time()),
			'task_id': task.id,
			'dataset_id': task.dataset_id,
			'task_type_id': task.task_type_id,
			'batch_number': batch_number,
			'pipeline_number': i,
			'reward_function_type': get_config('reward_function_type'),
			'sleep_time_after_new_actions': get_config('sleep_time_after_new_actions'),
			'max_actions_per_pipeline': get_config('max_actions_per_pipeline'),
			'mcts_iteration_limit': get_config('mcts_iteration_limit'),
			'target_action_count': get_config('target_action_count'),
			'expert_policy_model_name': get_config('expert_policy_model_name'),
			'expert_policy_probability': get_config('expert_policy_probability')
		}
		while not currentState.is_terminal():
			action = searcher.search(initialState=currentState)
			pipeline['actions'].append(action.get_dict())
			currentState = currentState.take_action(action)
			currentState.look_ahead_cnt = 0
			logger.info("(%d.%d) *** %s " % (i, currentState.depth, action))
			if len(pipeline['actions']) >= get_config('max_actions_per_pipeline'):
				pipeline['abort'] = True
				logger.info("Aborting pipeline")
				break
			if get_config('sleep_time_after_new_actions') > 0:
				logger.info("Sleeping for %1.2f" % get_config('sleep_time_after_new_actions'))
				time.sleep(get_config('sleep_time_after_new_actions'))
		pipeline['completed_at'] = math.floor(time.time())
		save_pipeline(pipeline)
