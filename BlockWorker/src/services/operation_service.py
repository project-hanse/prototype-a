from typing import Callable

import pandas as pd


class OperationService:
    def __init__(self, logging) -> None:
        self.logging = logging
        super().__init__()

    def get_operation_by_id(self, operation_id: str) -> Callable:
        self.logging.info('Getting operation %s' % operation_id)

        def operation(dataset: pd.DataFrame) -> pd.DataFrame:
            print(dataset)
            return dataset

        return operation
