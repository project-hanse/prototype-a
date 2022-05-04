from src.helper.operation_loader import OperationLoader


class HelperFactory:
    def __init__(self):
        self.operation_loader = None

    def get_operation_loader(self):
        if self.operation_loader is None:
            self.operation_loader = OperationLoader(api_base_url="https://hanse.allteams.at/api/pipeline",
                                                    api_user='api_user',
                                                    api_secret='d49e3f0f-964e-4241-9693-31d2c80e5ecd')
        return self.operation_loader
