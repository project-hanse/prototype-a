import logging


class LogHelper:
	loggers = {}

	@staticmethod
	def get_logger(name: str) -> logging:
		if name in LogHelper.loggers:
			return LogHelper.loggers[name]
		logger = logging.getLogger(name)
		if not logger.hasHandlers():
			handler = logging.StreamHandler()
			handler.setFormatter(logging.Formatter("%(asctime)s [%(levelname)s] %(message)s"))
			logger.addHandler(handler)
		logger.setLevel(logging.INFO)
		logger.propagate = False
		LogHelper.loggers[name] = logger
		return logger
