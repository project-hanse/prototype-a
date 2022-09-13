from src.models.dataset import Dataset


class DatasetDeleteEvent:

	def __init__(self, deserialized: dict):
		super().__init__()
		self._id: str = deserialized['Id']
		self._createdOn: str = deserialized['CreatedOn']
		self._dataset: Dataset = Dataset(deserialized['Dataset'])

	def get_dataset(self) -> Dataset:
		return self._dataset
