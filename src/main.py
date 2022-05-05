from mcts import mcts

from src.helper.helper_factory import HelperFactory
from src.pipeline_state import PipelineBuildingState

if __name__ == '__main__':
    currentState = PipelineBuildingState(helper_factory=HelperFactory(),
                                         available_datasets=[{'dataType': 2}, {'dataType': 1}, {'dataType': 5}])
    searcher = mcts(iterationLimit=100)

    while not currentState.isTerminal():
        action = searcher.search(initialState=currentState)
        currentState = currentState.takeAction(action)
        print("%s (%d)" % (action, currentState.depth))

