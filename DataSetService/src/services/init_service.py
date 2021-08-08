import threading

from src.helper.log_helper import LogHelper
from src.services.file_store import FileStore


class InitService:

    def __init__(self, file_store: FileStore) -> None:
        super().__init__()
        self.log = self.logger = LogHelper.get_logger('InitService')
        self.file_store = file_store

    def init_default_files_s3(self):
        """
        Makes sure that a number of default files (.csv, .xlsx) are available in the S3 file store.
        """
        self.log.info("Initializing files in S3 bucket")
        self.file_store.assert_bucket_exists()

    def init_default_files_s3_in_background(self):
        """
        Initializes the default data files in a dedicated thread.
        """
        import_thread = threading.Thread(target=self.init_default_files_s3(), name="Initializer")
        import_thread.start()
