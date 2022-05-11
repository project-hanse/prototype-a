import random
import uuid
from datetime import timedelta

import openml as openml
import requests_cache
from mcts import mcts

from src.config.config import load_open_ml_operation
from src.helper.expert_policy import model3_policy
from src.helper.helper_factory import HelperFactory
from src.pipeline_state import PipelineBuildingState

open_ml_task_ids = [31]


def get_initial_state():
    task = openml.tasks.get_task(random.choice(open_ml_task_ids))
    if task.task_type_id == openml.tasks.TaskType.SUPERVISED_CLASSIFICATION:
        return PipelineBuildingState(helper_factory=HelperFactory(),
                                     available_datasets=[{'dataType': 2, 'id': uuid.uuid4()},
                                                         {'dataType': 1, 'id': uuid.uuid4()}],
                                     producing_operation=load_open_ml_operation,
                                     max_look_ahead=10)

    raise Exception('Task type not supported')


if __name__ == '__main__':
    requests_cache.install_cache('mcts_cache',
                                 expire_after=timedelta(minutes=1),
                                 cache_control=False,
                                 allowable_methods=['GET', 'POST'])

    task = get_initial_state()

    currentState = get_initial_state()
    searcher = mcts(iterationLimit=20, rolloutPolicy=model3_policy)

    while not currentState.isTerminal():
        action = searcher.search(initialState=currentState)
        currentState = currentState.takeAction(action)
        currentState.look_ahead_cnt = 0
        print("(%d) *** %s " % (currentState.depth, action))
