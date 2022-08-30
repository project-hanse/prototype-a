import threading

from src.constants.bucket_names import DEFAULT_FILE_BUCKET_NAME
from src.helper.log_helper import LogHelper
from src.services.s3_wrapper import S3Wrapper


class InitService:

	def __init__(self, file_store: S3Wrapper) -> None:
		super().__init__()
		self.log = LogHelper.get_logger('InitService')
		self.file_store = file_store

	def init_default_files_s3(self):
		"""
		Makes sure that a number of default files (.csv, .xlsx) are available in the S3 file store.
		"""
		self.log.info("Initializing files in S3 bucket")
		self.file_store.assert_bucket_exists(DEFAULT_FILE_BUCKET_NAME)

		self.file_store.store_file_to_bucket("./datasets/21311-001Z_format.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/Melbourne_housing_FULL.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/MELBOURNE_HOUSE_PRICES_LESS.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/influenca_vienna_2009-2018.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/weather_vienna_2009-2018.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/21211-003Z_format.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/21311-001Z_format.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/simulated-vine-yield-styria.xlsx", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/monthly-beer-production-in-australia.csv",
																				 DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/wdbc.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/GlobalLandTemperatures_GlobalTemperatures.csv",
																				 DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/iris.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/per_game_metrics.csv", DEFAULT_FILE_BUCKET_NAME)
		self.file_store.store_file_to_bucket("./datasets/Month_Value_1_mod.csv", DEFAULT_FILE_BUCKET_NAME)

		# Store ZAMG data Graz Flughafen
		for year in range(1990, 2021):
			self.file_store.store_file_to_bucket(
				"./datasets/ZAMG/Steiermark/Graz_Flughafen/ZAMG_Jahrbuch_%s.csv" % year, DEFAULT_FILE_BUCKET_NAME)

	def init_default_files_s3_in_background(self):
		"""
		Initializes the default data files in a dedicated thread.
		"""
		import_thread = threading.Thread(target=self.init_default_files_s3, name="Initializer")
		import_thread.start()
