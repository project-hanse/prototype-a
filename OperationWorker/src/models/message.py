import json


def snake_to_camel(s):
	a = s.split('_')
	a[0] = a[0].lower()
	if len(a) > 1:
		a[1:] = [u.title() for u in a[1:]]
	return ''.join(a)


def snake_to_pascal(s):
	a = s.split('_')
	a[0:] = [u.title() for u in a[0:]]
	return ''.join(a)


def serialise(obj):
	return {snake_to_pascal(k): v for k, v in obj.__dict__.items()}


class Message:
	def __init__(self):
		return

	def to_json(self):
		return json.dumps(serialise(self), indent=4, sort_keys=True, default=str)
