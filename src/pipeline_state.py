from itertools import product

from src.helper.helper_factory import HelperFactory


class PipelineBuildingState:
    def __init__(self, available_datasets):
        self.available_datasets = available_datasets

    def getCurrentPlayer(self):
        return 1

    def getPossibleActions(self):
        operation_loader = HelperFactory().get_operation_loader()
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
                            Action(operation_name=operation["operationName"], input_vector=datasets_comb_datatype_vec))
        return actions

    def takeAction(self, action):
        pass

    def isTerminal(self):
        return False

    def getReward(self):
        return False

    def get_datatype_vector(self, dataset_combination):
        vector = []
        for dataset in dataset_combination:
            vector.append(dataset['dataType'])
        return vector


class Action:
    def __init__(self, operation_name: str, input_vector: list):
        self.input_vector = input_vector
        self.operation_name = operation_name

    def __str__(self):
        return self.operation_name + " " + str(self.input_vector)

    def __repr__(self):
        return str(self)

    def __eq__(self, other):
        return self.__class__ == other.__class__ and self.operation_name == other.operation_name and self.input_vector == other.input_vector

    def __hash__(self):
        return hash(hash(self.operation_name) + hash(self.input_vector))
