import math
import random
import uuid
from itertools import product

from mcts.base.base import BaseState, BaseAction

from src.config.config import get_terminal_operation_ids, max_dataset_inputs_per_operation
from src.helper.helper_factory import HelperFactory


def are_vectors_equal(vec_a, vec_b) -> bool:
    return len(vec_a) == len(vec_b) and all(x == y for x, y in zip(vec_b, vec_a))


class PipelineBuildingState(BaseState):
    def __init__(self, helper_factory: HelperFactory, available_datasets: [{}], producing_operation: {},
                 max_look_ahead: int = None, look_ahead_cnt: int = 0, max_actions: int = None,
                 max_available_datasets: int = None, depth: int = 0, parent: 'PipelineBuildingState' = None,
                 verbose: float = 0):
        # The operation that produces the newly added dataset
        self.producing_operation: {} = producing_operation
        # All datasets (including the newly computed dataset) that are available for the current state
        self.available_datasets = available_datasets
        self.verbose = verbose

        if self.print():
            print("%s (%s)" % (self.producing_operation['operationName'], str(self.available_datasets)))

        self.helper_factory = helper_factory
        self.depth = depth
        self.look_ahead_cnt = look_ahead_cnt
        self.max_look_ahead = max_look_ahead
        self.max_actions = max_actions
        self.max_available_datasets = max_available_datasets
        self.parent = parent
        self.terminal_operation_ids = get_terminal_operation_ids()

    def get_current_player(self) -> int:
        # Always maximizing player
        return 1

    def get_possible_actions(self) -> []:
        operation_loader = self.helper_factory.get_operation_loader()
        actions = []
        # get all possible combinations of available_datasets
        for k in range(min(len(self.available_datasets), max_dataset_inputs_per_operation)):
            k += 1
            # print("k(%d) -> %d" % (len(self.available_datasets), k))
            for dataset_combination in product(self.available_datasets, repeat=k):
                for operation in operation_loader.load_operations():
                    datasets_comb_datatype_vec = self.get_datatype_vector(dataset_combination)
                    op_input_datatype_vec = operation['inputTypes']
                    # element wise comparison
                    if are_vectors_equal(datasets_comb_datatype_vec, op_input_datatype_vec):
                        actions.append(Action(operation=operation, input_datasets=dataset_combination))
        if actions is None or len(actions) == 0:
            print("No actions available")
            return []
        if self.print():
            print("%s actions available" % len(actions))
        # maximum n possible (random) actions to reduce search tree size
        if self.max_actions is not None:
            actions = random.sample(actions, min(len(actions), self.max_actions))
        random.shuffle(actions)
        return actions

    def take_action(self, action):
        new_available_datasets = []

        # reduce search space by not adding datasets to available datasets where operation input vector and output
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

        if self.max_available_datasets is not None:
            # take maximum n random available datasets to reduce search tree size (TODO: change to n most recent)
            # and make sure all dataset types are covered
            new_available_datasets = random.sample(new_available_datasets,
                                                   min(len(new_available_datasets), self.max_available_datasets))

        new_state = PipelineBuildingState(helper_factory=self.helper_factory,
                                          available_datasets=new_available_datasets,
                                          producing_operation=action.operation,
                                          max_look_ahead=self.max_look_ahead,
                                          max_available_datasets=self.max_available_datasets,
                                          max_actions=self.max_actions,
                                          look_ahead_cnt=self.look_ahead_cnt + 1,
                                          # TODO: make depth dependent on amount of preceding operations not depth of search tree
                                          depth=self.depth + 1,
                                          verbose=self.verbose,
                                          parent=self)

        return new_state

    def is_terminal(self) -> bool:
        # TODO: change this to a more sophisticated check (e.g. assembly index)
        if not self.available_datasets:
            return True
        if self.look_ahead_cnt == self.max_look_ahead:
            # abort search if maximum look ahead is reached
            return True
        if self.producing_operation is not None:
            if self.producing_operation['operationId'] in self.terminal_operation_ids:
                # print("Terminal operation (%s) reached" % self.producing_action.operation['operationName'])
                return True
        return False

    def get_reward(self) -> float:
        if self.producing_operation is None:
            return 0
        if self.look_ahead_cnt >= self.max_look_ahead:
            # no reward if maximum look ahead is reached
            return 0
        if self.producing_operation['operationId'] in self.terminal_operation_ids:
            return self.reward_function(self.depth)
        return False

    def reward_function(self, depth):
        # punish greater depth
        negative_reward_from = 5
        return (negative_reward_from / (negative_reward_from * (math.log(depth, negative_reward_from) + 1))) - 1

    def get_datatype_vector(self, dataset_combination):
        vector = []
        for dataset in dataset_combination:
            vector.append(dataset['type'])
        return vector

    def print(self) -> bool:
        return self.verbose > 0 and random.random() < self.verbose


class Action(BaseAction):
    def __init__(self, operation: dict, input_datasets: [{}]):
        # the operation that is added to the pipeline
        self.operation = operation
        # the datasets that are used in the operation
        self.input_datasets = input_datasets

    def get_dict(self) -> dict:
        return {'operation': self.operation, 'input_datasets': self.input_datasets}

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
            new_datasets.append({'type': output_type, 'id': uuid.uuid4()})
        return new_datasets
