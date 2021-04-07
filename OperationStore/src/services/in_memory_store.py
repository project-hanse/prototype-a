import dill


class InMemoryStore:
    def __init__(self, logger) -> None:
        self.logger = logger
        self.operations = {}
        super().__init__()

    def get_by_id(self, operation_id):
        if operation_id in self.operations:
            self.logger.info("Loading operation with id %s" % operation_id)
            return dill.loads(self.operations[operation_id])

        self.logger.info("Operation with id %s not found" % operation_id)
        return None

    def get_operation_count(self):
        return len(self.operations)

    def generate_defaults(self):
        self.logger.info("Generating default operations...")
        # TODO