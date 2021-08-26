class OperationsHelper:

    @staticmethod
    def get_or_default(operation_config: dict, key: str, default):
        if key in operation_config:
            return operation_config[key]
        else:
            return default
