import math
import random
from itertools import product

from src.config.config import get_terminal_operation_ids
from src.helper.helper_factory import HelperFactory


class PipelineBuildingState:
    def __init__(self, helper_factory: HelperFactory, available_datasets, depth: int = 0, producing_action=None,
                 parent=None):
        self.helper_factory = helper_factory
        self.available_datasets = available_datasets
        self.depth = depth
        self.producing_action: Action = producing_action
        self.parent = parent
        self.terminal_operation_ids = get_terminal_operation_ids()

    def getCurrentPlayer(self):
        return 1

    def getPossibleActions(self):
        operation_loader = self.helper_factory.get_operation_loader()
        actions = []
        # get all possible combinations of available_datasets
        for k in range(len(self.available_datasets)):
            for dataset_combination in product(self.available_datasets, repeat=(k + 1)):
                for operation in operation_loader.load_operations():
                    datasets_comb_datatype_vec = self.get_datatype_vector(dataset_combination)
                    op_input_datatype_vec = operation['inputTypes']
                    # element wise comparison
                    if len(datasets_comb_datatype_vec) == len(op_input_datatype_vec) and all(
                            x == y for x, y in zip(op_input_datatype_vec, datasets_comb_datatype_vec)):
                        actions.append(
                            Action(operation=operation,
                                   datasets=dataset_combination,
                                   input_vector=datasets_comb_datatype_vec))
        if actions is None or len(actions) == 0:
            print("No actions available")
            return []
        random.shuffle(actions)
        return actions

    def takeAction(self, action):
        new_state = PipelineBuildingState(helper_factory=self.helper_factory,
                                          available_datasets=action.datasets,
                                          producing_action=action,
                                          # TODO: make depth dependent on amount of preceding operations not depth of search tree
                                          depth=self.depth + 1,
                                          parent=self)

        return new_state

    def isTerminal(self):
        # TODO: change this to a more sophisticated check (e.g. assembly index, check for OpenML result)
        if not self.available_datasets:
            return True
        if self.producing_action is not None:
            if self.producing_action.operation['operationId'] in self.terminal_operation_ids:
                # print("Terminal operation (%s) reached" % self.producing_action.operation['operationName'])
                return True
        return False

    def getReward(self):
        if self.isTerminal():
            return False
        if self.producing_action is None:
            return 0
        if self.producing_action.operation['operationId'] in self.terminal_operation_ids:
            # punish greater depth
            return 1 / (math.log(self.depth) + 1)
        return False

    def get_datatype_vector(self, dataset_combination):
        vector = []
        for dataset in dataset_combination:
            vector.append(dataset['dataType'])
        return vector


class Action:
    def __init__(self, operation: dict, datasets: [{}], input_vector: list):
        self.input_vector = input_vector
        self.operation = operation
        self.datasets = datasets

    def __repr__(self):
        return str(self)

    def __str__(self):
        return "%s -> %s -> %s" % (
            str(self.input_vector), str(self.operation['operationName']), str(self.operation['outputTypes']))

    def __eq__(self, other):
        return self.__class__ == other.__class__ and self.input_vector == other.input_vector and self.operation == other.operation and self.datasets == other.datasets

    def __hash__(self):
        return hash(str(self.operation)) + hash(str(self.input_vector)) + hash(str(self.datasets))

    def get_output_vector(self):
        return self.operation['outputTypes']
