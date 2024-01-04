class AggregateException(Exception):
	def __init__(self, message, exceptions: []):
		self.exceptions = exceptions
		super().__init__(message)
