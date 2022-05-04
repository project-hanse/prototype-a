import requests as requests


class OperationLoader:
    def __init__(self, api_base_url: str, api_user: str, api_secret: str):
        self.api_base_url = api_base_url
        self.auth = (api_user, api_secret)
        self.operations_cache = None

    def load_operations(self) -> list:
        if self.operations_cache is None:
            response = requests.get(self.api_base_url + "/api/v1/operationTemplates", auth=self.auth)
            response.raise_for_status()
            self.operations_cache = response.json()
        return self.operations_cache
