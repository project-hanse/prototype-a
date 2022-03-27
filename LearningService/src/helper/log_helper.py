import logging


class LogHelper:

	@staticmethod
	def get_logger(name: str) -> logging:
		logger = logging.getLogger(name)
		handler = logging.StreamHandler()
		handler.setFormatter(logging.Formatter("%(asctime)s [%(levelname)s] %(message)s"))
		logger.addHandler(handler)
		logger.setLevel(logging.INFO)
		return logger
