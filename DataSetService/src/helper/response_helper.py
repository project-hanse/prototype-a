import pandas as pd
from flask import Flask, abort, jsonify


def format_response(data, format_):
	if format_ == 'json' or format_ is None:
		return format_json(data)
	elif format_ == 'csv':
		return format_csv(data)
	elif format_ == 'html':
		return format_html(data)
	else:
		abort(400)


def format_json(data):
	if type(data) is dict:
		return jsonify(data)
	elif type(data) is str:
		return data
	elif type(data) is pd.DataFrame:
		return serialize_dataframe(data)
	elif type(data) is pd.Series:
		return serialize_series(data)
	else:
		abort(400)


def format_csv(data):
	if type(data) is dict:
		return abort(500)
	elif type(data) is str:
		return data
	elif type(data) is pd.DataFrame:
		return data.to_csv()
	elif type(data) is pd.Series:
		return data.to_frame().to_csv()
	else:
		abort(500)


def format_html(data):
	if type(data) is dict:
		return abort(400)
	elif type(data) is str:
		return data
	elif type(data) is pd.DataFrame:
		return data.to_html()
	elif type(data) is pd.Series:
		return data.to_frame().to_html()
	else:
		abort(400)


def serialize_dataframe(df: pd.DataFrame):
	return Flask.response_class(
		response=df.to_json(date_format='iso'),
		status=200,
		mimetype='application/json'
	)


def serialize_series(df: pd.Series):
	return Flask.response_class(
		response=df.to_json(date_format='iso'),
		status=200,
		mimetype='application/json'
	)
