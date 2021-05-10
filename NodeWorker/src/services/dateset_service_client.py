import pandas as pd
import requests

from src.exceptions.NotFoundError import NotFoundError
from src.exceptions.NotStoredError import NotStoredError


class DatasetServiceClient:
    host: str
    port: int

    def __init__(self, host: str, port: int, logging) -> None:
        self.host = host
        self.port = port
        self.logging = logging

    def get_dataset_by_id(self, dataset_id: str) -> pd.DataFrame:
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/' + dataset_id
        self.logging.info('Loading dataset from %s' % address)
        response = requests.get(address)
        if response.status_code == 404:
            raise NotFoundError("No dataset with id found")
        return pd.read_json(response.text)

    def get_dataset_by_hash(self, producing_hash: str) -> pd.DataFrame:
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/hash/' + producing_hash
        self.logging.info('Loading dataset from %s' % address)
        response = requests.get(address)
        if response.status_code == 404:
            raise NotFoundError("No dataset with hash found")
        return pd.read_json(response.text)

    def store_with_hash(self, producing_hash: str, resulting_dataset: pd.DataFrame):
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/hash/' + producing_hash
        self.logging.info('Storing dataset to %s' % address)
        response = requests.post(address, data=resulting_dataset.to_json())
        if response.status_code < 300:
            self.logging.info('Store responded with status code (%i) %s' % (response.status_code, str(response.reason)))
        else:
            self.logging.warning('Failed to store dataset: (%i) %s' % (response.status_code, str(response.text)))
            raise NotStoredError('Failed to store dataset: (%i) %s' % (response.status_code, str(response.reason)))
