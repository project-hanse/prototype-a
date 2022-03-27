def get_type_str(obj_type: type) -> str:
	try:
		return obj_type.__name__.lower().split('.')[-1]
	except Exception as e:
		try:
			return str(obj_type).lower().split('.')[-1]
		except Exception as e:
			return str(obj_type).lower()
