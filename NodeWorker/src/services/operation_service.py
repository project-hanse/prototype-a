import logging
from typing import Callable

import pandas as pd

from src.services.operations_collection import OperationsCollection


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
        self.local_operations["0759dede-2cee-433c-b314-10a8fa456e62"] = OperationsCollection.pd_single_input_generic
        self.local_operations[
            "7b0bb47f-f997-43d8-acb1-c31f2a22475d"] = OperationsCollection.pd_single_input_select_columns
        self.local_operations["d2701fa4-b038-4fcb-b981-49f9f123da01"] = OperationsCollection.pd_single_input_select_rows
        self.local_operations["5c9b34fc-ac4f-4290-9dfe-418647509559"] = OperationsCollection.pd_single_input_trim_rows
        self.local_operations[
            "db8b6a9d-d01f-4328-b971-fa56ac350320"] = OperationsCollection.pd_single_input_make_row_header
        self.local_operations["9acea312-713e-4de8-b8db-5d33613ab2f1"] = OperationsCollection.pd_double_input_join

    def get_simple_operation_by_id(self, operation_id: str) -> Callable:
        self.logger.info('Getting simple operation %s' % operation_id)
        if operation_id in self.local_operations:
            return self.local_operations[operation_id]

        self.logger.warn("Operation with id %s not found" % operation_id)
        raise NotImplementedError("Operation with this id is not implemented")
