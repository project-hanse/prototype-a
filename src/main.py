from datetime import timedelta

import requests_cache
from mcts import mcts

from src.helper.expert_policy import model3_policy
from src.helper.helper_factory import HelperFactory
from src.pipeline_state import PipelineBuildingState

if __name__ == '__main__':
    requests_cache.install_cache('mcts_cache',
                                 expire_after=timedelta(days=1),
                                 cache_control=False,
                                 allowable_methods=['GET', 'POST'])

    currentState = PipelineBuildingState(helper_factory=HelperFactory(),
                                         available_datasets=[{'dataType': 2}, {'dataType': 1}, {'dataType': 5}])
    searcher = mcts(iterationLimit=100, rolloutPolicy=model3_policy)

    while not currentState.isTerminal():
        action = searcher.search(initialState=currentState)
        currentState = currentState.takeAction(action)
        print("%s (%d)" % (action, currentState.depth))
