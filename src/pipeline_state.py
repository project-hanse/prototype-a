import math
import random
import uuid
from itertools import product

from src.config.config import get_terminal_operation_ids, max_dataset_inputs_per_operation
from src.helper.helper_factory import HelperFactory


def are_vectors_equal(vec_a, vec_b) -> bool:
    return len(vec_a) == len(vec_b) and all(x == y for x, y in zip(vec_b, vec_a))


class PipelineBuildingState:
    def __init__(self, helper_factory: HelperFactory, available_datasets: [{}], producing_operation: {},
                 depth: int = 0, parent: 'PipelineBuildingState' = None):
        # The operation that produces the newly added dataset
        self.producing_operation: {} = producing_operation
        # All datasets (including the newly computed dataset) that are available for the current state
        self.available_datasets = available_datasets

        if random.random() < 0.1:
            print("%s (%s)" % (self.producing_operation['operationName'], str(self.available_datasets)))

        self.helper_factory = helper_factory
        self.depth = depth
        self.parent = parent
        self.terminal_operation_ids = get_terminal_operation_ids()

    def getCurrentPlayer(self):
        # Always maximizing player
        return 1

    def getPossibleActions(self):
        operation_loader = self.helper_factory.get_operation_loader()
        actions = []
        # get all possible combinations of available_datasets
        for k in range(min(len(self.available_datasets), max_dataset_inputs_per_operation)):
            k += 1
            # print("k(%d) -> %d" % (len(self.available_datasets), (k)))
            for dataset_combination in product(self.available_datasets, repeat=(k)):
                for operation in operation_loader.load_operations():
                    datasets_comb_datatype_vec = self.get_datatype_vector(dataset_combination)
                    op_input_datatype_vec = operation['inputTypes']
                    # element wise comparison
                    if are_vectors_equal(datasets_comb_datatype_vec, op_input_datatype_vec):
                        actions.append(Action(operation=operation, input_datasets=dataset_combination))
        if actions is None or len(actions) == 0:
            print("No actions available")
            return []
        if random.random() < 0.1:
            print("%s actions available" % len(actions))
        random.shuffle(actions)
        return actions

    def takeAction(self, action):
        new_available_datasets = []

        # reduce search space by not adding datasets to available datasets where operation input vector and outpue
        # vector are the same - it is likely that only the results of the previous operation are relevant
        input_vector = self.get_datatype_vector(action.input_datasets)
        output_vector = self.get_datatype_vector(action.get_resulting_datasets())
        if are_vectors_equal(input_vector, output_vector):
            new_available_datasets.extend(action.get_resulting_datasets())
            for available_dataset in self.available_datasets:
                # only add a dataset to the newly available if it is not an input of the operation (= processed by the
                # operation)
                if available_dataset['id'] not in [dataset['id'] for dataset in action.input_datasets]:
                    new_available_datasets.append(available_dataset)
        else:
            # keep all existing available datasets + the newly computed dataset
            new_available_datasets.extend(self.available_datasets)
            new_available_datasets.extend(action.get_resulting_datasets())

        new_state = PipelineBuildingState(helper_factory=self.helper_factory,
                                          available_datasets=new_available_datasets,
                                          producing_operation=action.operation,
                                          # TODO: make depth dependent on amount of preceding operations not depth of search tree
                                          depth=self.depth + 1,
                                          parent=self)

        return new_state

    def isTerminal(self):
        # TODO: change this to a more sophisticated check (e.g. assembly index, check for OpenML result)
        if not self.available_datasets:
            return True
        if self.producing_operation is not None:
            if self.producing_operation['operationId'] in self.terminal_operation_ids:
                # print("Terminal operation (%s) reached" % self.producing_action.operation['operationName'])
                return True
        return False

    def getReward(self):
        if self.isTerminal():
            return False
        if self.producing_operation is None:
            return 0
        if self.producing_operation['operationId'] in self.terminal_operation_ids:
            # punish greater depth
            return 1 / (math.log(self.depth) + 1)
        return False

    def get_datatype_vector(self, dataset_combination):
        vector = []
        for dataset in dataset_combination:
            vector.append(dataset['dataType'])
        return vector


class Action:
    def __init__(self, operation: dict, input_datasets: [{}]):
        # the operation that is added to the pipeline
        self.operation = operation
        # the datasets that are used in the operation
        self.input_datasets = input_datasets

    def __repr__(self):
        return str(self)

    def __str__(self):
        return "%s -> %s -> %s" % (
            str(self.operation['inputTypes']), str(self.operation['operationName']), str(self.operation['outputTypes']))

    def __eq__(self, other):
        return self.__class__ == other.__class__ and self.operation == other.operation and self.input_datasets == other.input_datasets

    def __hash__(self):
        return hash(str(self.operation)) + hash(str(self.input_datasets))

    def get_output_vector(self):
        return self.operation['outputTypes']

    def get_resulting_datasets(self) -> [{}]:
        new_datasets = []
        for output_type in self.operation['outputTypes']:
            new_datasets.append({'dataType': output_type, 'id': uuid.uuid4()})
        return new_datasets
