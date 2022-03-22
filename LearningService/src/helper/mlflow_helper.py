import logging

from mlflow.models import infer_signature, ModelSignature
from mlflow.types.schema import Schema, ColSpec, DataType
from mlflow.types.utils import _infer_schema


def _get_schema_datatype(value: any) -> DataType:
	"""
	Get the datatype of the value.
	"""
	if isinstance(value, int):
		return DataType.integer
	elif isinstance(value, float):
		return DataType.float
	elif isinstance(value, str):
		return DataType.string
	elif isinstance(value, bool):
		return DataType.boolean
	else:
		return DataType.string


def _infer_signature_custom(model_input, model_output):
	"""
	Infer the signature of the model.
	Tries to use mlflow.infer_signature(), if this does not work try custom implementation for dicts.
	"""
	if isinstance(model_input, dict):
		input_cols = []
		for key, val in model_input.items():
			input_cols.append(ColSpec(_get_schema_datatype(val), key))
		try:
			output_schema = _infer_schema(model_output)
		except Exception as e:
			output_schema = Schema([ColSpec(_get_schema_datatype(model_output), "predicted_label")])
		return ModelSignature(inputs=Schema(input_cols), outputs=output_schema)
	else:
		logging.warning("Could not infer signature.")
		return None


def infer_signature_custom(model_input, model_output):
	"""
	Infer the signature of the model.
	Tries to use mlflow.infer_signature(), if this does not work try custom implementation for dicts.
	"""
	try:
		return infer_signature(model_input, model_output)
	except Exception as e:
		logging.info("Could not infer signature. Trying custom implementation.")
		try:
			return _infer_signature_custom(model_input, model_output)
		except Exception as e:
			logging.warning("Could not infer signature. Returning None.")
			return None
