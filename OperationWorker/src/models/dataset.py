from src.models.dataset_type import DatasetType


class Dataset:
	def __init__(self, deserialized: dict):
		self.dataset_type: DatasetType = DatasetType(deserialized['Type'])
		self.key: str = deserialized['Key']
		self.store: str = deserialized['Store']

	def get_type(self) -> DatasetType:
		return self.dataset_type

	def get_key(self) -> str:
		return self.key

	def get_store(self) -> str:
		return self.store

	@staticmethod
	def from_array(dataset_dicts: []) -> []:
		result = []
		for dataset_dict in dataset_dicts:
			result.append(Dataset(dataset_dict))
		return result
