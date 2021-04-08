import logging

import dill
import pandas as pd


class InMemoryStore:
    def __init__(self, logger: logging) -> None:
        self.logger = logger
        self.operations = {}
        super().__init__()

    def get_by_id(self, operation_id):
        if operation_id in self.operations:
            self.logger.info("Loading operation with id %s" % operation_id)
            return self.operations[operation_id]

        self.logger.info("Operation with id %s not found" % operation_id)
        return None

    def get_operation_count(self):
        return len(self.operations)

    def generate_defaults(self):
        self.logger.info("Generating default operations...")
        self.operations["simple_pd_drop"] = dill.dumps(InMemoryStore.simple_pd_dropna)

    @staticmethod
    def simple_pd_dropna(operation_config: dict, logger: logging, df: pd.DataFrame):
        logger.info("Config", operation_config)
        logger.info("Df:", df)
        df.dropna(**operation_config)
        logger.info("Operation dropna completed")
