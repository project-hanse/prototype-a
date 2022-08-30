import pandas as pd


def get_str_from_type(obj_type: type) -> str:
	try:
		return obj_type.__name__.lower().split('.')[-1]
	except Exception as e:
		try:
			return str(obj_type).lower().split('.')[-1]
		except Exception as e:
			return str(obj_type).lower()


def get_type_from_str(obj_type: str) -> type:
	if obj_type == 'int':
		return int
	elif obj_type == 'float':
		return float
	elif obj_type == 'str':
		return str
	elif obj_type == 'bool':
		return bool
	elif obj_type == 'dataframe':
		return pd.DataFrame
	elif obj_type == 'series':
		return pd.Series


def get_metadata_key(key: str, metadata_version: str) -> str:
	return '{}.{}'.format(key, metadata_version)
