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
        self.local_operations["0759dede-2cee-433c-b314-10a8fa456e62"] = OperationsCollection.simple_pd_generic
        self.local_operations["7b0bb47f-f997-43d8-acb1-c31f2a22475d"] = OperationsCollection.simple_pd_select_columns

    def get_simple_operation_by_id(self, operation_id: str) -> Callable:
        self.logger.info('Getting simple operation %s' % operation_id)
        if operation_id in self.local_operations:
            return self.local_operations[operation_id]

        self.logger.warn("Operation with id %s not found" % operation_id)
        raise NotImplementedError("Operation with this id is not implemented")
