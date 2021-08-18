import logging
from typing import Callable

import pandas as pd

from src.services.operations_file_input import OperationsFileInputCollection
from src.services.operations_single_input_collection import OperationsSingleInputCollection


class OperationService:

    @staticmethod
    def operation(dataset: pd.DataFrame) -> pd.DataFrame:
        print("dataset:", dataset)
        return dataset

    def __init__(self, host: str, port: int, logger: logging) -> None:
        self.logger = logger
        self.host = host
        self.port = port
        self.local_operations = {}
        super().__init__()

    def init(self):
        self.logger.info("Initializing local operations store...")

        self.local_operations[
            "dfbca055-69f1-40df-9639-023ec6363bac"] = OperationsFileInputCollection.pd_file_input_read_csv

        self.local_operations[
            "0ebc4dd5-6a81-48e7-8abd-3488c608020f"] = OperationsSingleInputCollection.pd_single_input_transpose
        self.local_operations[
            "0759dede-2cee-433c-b314-10a8fa456e62"] = OperationsSingleInputCollection.pd_single_input_generic
        self.local_operations[
            "de26c7a0-0444-414d-826f-458cd3b8979c"] = OperationsSingleInputCollection.pd_single_input_set_index
        self.local_operations[
            "e44cc87e-3150-4387-b5dc-f7a7b8131d87"] = OperationsSingleInputCollection.pd_single_input_reset_index
        self.local_operations[
            "0fb2b572-bc3c-48d5-9c31-6bf0d0f7cc61"] = OperationsSingleInputCollection.pd_single_input_rename
        self.local_operations[
            "43f6b64a-ae47-45e3-95e5-55dc65d4249e"] = OperationsSingleInputCollection.pd_single_input_drop
        self.local_operations[
            "074669e8-9b60-48ce-bfc9-509d5990f517"] = OperationsSingleInputCollection.pd_single_input_mean
        self.local_operations[
            "7b0bb47f-f997-43d8-acb1-c31f2a22475d"] = OperationsSingleInputCollection.pd_single_input_select_columns
        self.local_operations[
            "d2701fa4-b038-4fcb-b981-49f9f123da01"] = OperationsSingleInputCollection.pd_single_input_select_rows
        self.local_operations[
            "5c9b34fc-ac4f-4290-9dfe-418647509559"] = OperationsSingleInputCollection.pd_single_input_trim_rows
        self.local_operations[
            "db8b6a9d-d01f-4328-b971-fa56ac350320"] = OperationsSingleInputCollection.pd_single_input_make_row_header

        self.local_operations[
            "9acea312-713e-4de8-b8db-5d33613ab2f1"] = OperationsSingleInputCollection.pd_double_input_join

    def get_simple_operation_by_id(self, operation_id: str) -> Callable:
        self.logger.info('Getting simple operation %s' % operation_id)
        if operation_id in self.local_operations:
            return self.local_operations[operation_id]

        self.logger.warn("Operation with id %s not found" % operation_id)
        raise NotImplementedError("Operation with this id is not implemented")

    def get_file_operation_by_id(self, operation_id) -> Callable:
        self.logger.info('Getting operation %s' % operation_id)
        if operation_id in self.local_operations:
            return self.local_operations[operation_id]

        self.logger.warn("Operation with id %s not found" % operation_id)
        raise NotImplementedError("Operation with this id is not implemented")
