import requests
import pandas as pd


class DatasetServiceClient:
    host: str
    port: int

    def __init__(self, host: str, port: int, logging) -> None:
        self.host = host
        self.port = port
        self.logging = logging

    def get_dataset_by_id(self, dataset_id: str):
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/' + dataset_id
        self.logging.info('Loading dataset from %s' % address)
        data = requests.get(address)
        return pd.read_json(data.text)

    def get_dataset_by_hash(self, producing_hash: str):
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/hash/' + producing_hash
        self.logging.info('Loading dataset from %s' % address)
        data = requests.get(address)
        return pd.read_json(data.text)

    def store_with_hash(self, producing_hash: str, resulting_dataset):
        address = 'http://' + self.host + ':' + str(self.port) + '/api/datasets/hash/' + producing_hash
        self.logging.info('Storing dataset to %s' % address)
        requests.post(address, json=resulting_dataset.values.tolist())
