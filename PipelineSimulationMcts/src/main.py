import json
import math
import random
import time
import uuid
from datetime import timedelta

import openml as openml
import requests_cache
from mcts.searcher.mcts import MCTS
from openml import OpenMLTask

from src.config.config import *
from src.helper.expert_policy import model3_policy
from src.helper.helper_factory import HelperFactory
from src.helper.serializer import TMCSerializer
from src.model.pipeline_state import PipelineBuildingState


def get_initial_state() -> (OpenMLTask, PipelineBuildingState):
	task = openml.tasks.get_task(random.choice(open_ml_task_ids))
	if task.task_type_id == openml.tasks.TaskType.SUPERVISED_CLASSIFICATION:
		state = PipelineBuildingState(helper_factory=HelperFactory(),
																	available_datasets=[{'type': 2, 'key': uuid.uuid4()},
																											{'type': 1, 'key': uuid.uuid4()}],
																	producing_operation=load_open_ml_operation,
																	max_look_ahead=max_look_ahead_steps,
																	verbose=verbose_level)
		state.producing_operation['defaultConfig']['data_id'] = task.dataset_id
		return task, state

	raise Exception('Task type not supported')


def save_pipeline(pipeline: dict):
	os.makedirs(pipelines_dir, exist_ok=True)
	with open(os.path.join(pipelines_dir, 'pipeline-%s.json' % math.floor(time.time())), 'w') as f:
		json.dump(pipeline, f, cls=TMCSerializer)


if __name__ == '__main__':
	init_config()
	requests_cache.install_cache('mcts_cache',
															 expire_after=timedelta(minutes=1),
															 cache_control=False,
															 allowable_methods=['GET', 'POST'])

	searcher = MCTS(iterationLimit=mcts_iteration_limit, rolloutPolicy=model3_policy)
	batch_number = random.randint(0, 10000)
	for i in range(pipeline_iterations):
		task, currentState = get_initial_state()
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
			'pipeline_number': i
		}
		while not currentState.is_terminal():
			action = searcher.search(initialState=currentState)
			pipeline['actions'].append(action.get_dict())
			currentState = currentState.take_action(action)
			currentState.look_ahead_cnt = 0
			print("(%d.%d) *** %s " % (i, currentState.depth, action))
			if len(pipeline['actions']) > max_actions_per_pipeline:
				pipeline['abort'] = True
				print("Aborting pipeline")
				break
			time.sleep(sleep_time_after_new_actions)
		pipeline['completed_at'] = math.floor(time.time())
		save_pipeline(pipeline)
