from sklearn.datasets import fetch_openml


def custom_fetch_openml(name: str, version: str, data_id: int, data_home: str, target_column: str, cache: bool,
												return_X_y: bool, as_frame: bool) -> []:
	"""
	Fetch dataset from openml by name or dataset id.
	"""
	data, target = fetch_openml(name=name,
															version=version,
															data_id=data_id,
															data_home=data_home,
															target_column=target_column,
															cache=cache,
															return_X_y=return_X_y,
															as_frame=as_frame)
	return [data, target]
