from src.pipeline_state import PipelineBuildingState

if __name__ == '__main__':
    initial_state = PipelineBuildingState([{'dataType': 2}, {'dataType': 1}, {'dataType': 5}])
    print(initial_state.getPossibleActions())
    # searcher = mcts(timeLimit=1000)
    # resultDict = searcher.search(initialState=initial_state)
    # print(resultDict)  # currently includes dict_keys(['action', 'expectedReward'])
