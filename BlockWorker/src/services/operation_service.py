import base64
import json
import logging
from typing import Callable

import dill
import pandas as pd
import requests


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

    def store(self):
        self.local_operations["id"] = dill.dumps(OperationService.operation)

    def get_simple_operation_by_id(self, operation_id: str) -> Callable:
        self.logger.info('Getting simple operation %s' % operation_id)
        if operation_id in self.local_operations:
            operation = dill.loads(self.local_operations[operation_id])
        else:
            self.logger.warn("Operation with id %s not found" % operation_id)
            operation = self.get_from_remote(operation_id)
        return operation

    # TODO this is bullshit and can be replaced by local implementations of operations and associating functions with ids
    def get_from_remote(self, operation_id: str) -> Callable:
        address = 'http://' + self.host + ':' + str(self.port) + '/api/operations/' + operation_id
        self.logger.info('Loading dataset from %s' % address)
        response = requests.get(address)
        if response.status_code == 200:
            content = json.loads(response.text)
            return dill.loads(bytes.fromhex(content["serializedOperation"]))
        self.logger.warn(
            "Operations service responded with status code %i and error %s" % (response.status_code, response.text))

        return None


# TODO remove me
if __name__ == '__main__':
    logging.basicConfig(
        level=logging.DEBUG,
        format="%(asctime)s [%(levelname)s] %(message)s",
        handlers=[
            logging.StreamHandler()
        ]
    )
    service = OperationService("localhost", 5010, logging)
    service.store()
    operation = service.get_simple_operation_by_id("simple_pd_drop")

    operation(None)
