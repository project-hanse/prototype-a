import threading

import boto3
from botocore.exceptions import ClientError

from src.helper.log_helper import LogHelper


class InitService:

    def __init__(self) -> None:
        super().__init__()
        self.log = self.logger = LogHelper.get_logger('InitService')
        self.session = None
        self.s3_client = None

    def setup(self, s3_endpoint: str, s3_access_key_id: str, s3_secret_access_key: str):
        self.log.info("Setting up client for S3 service at %s" % s3_endpoint)
        try:
            self.session = boto3.session.Session()
            self.s3_client = self.session.client(
                service_name='s3',
                aws_access_key_id=s3_access_key_id,
                aws_secret_access_key=s3_secret_access_key,
                endpoint_url=s3_endpoint
            )
        except Exception as e:
            self.log.error("Failed to setup connection to S3 service %s" % str(e))

    def init_default_files_s3(self):
        """
        Makes sure that a number of default files (.csv, .xlsx) are available in the S3 file store.
        """
        self.log.info("Initializing files in S3 bucket")
        try:
            response = self.s3_client.list_buckets()
            self.log.info("Available S3 buckets: %s" % response['Buckets'])
        except ClientError as e:
            self.log.error(e)

    def init_default_files_s3_in_background(self):
        """
        Initializes the default data files in a dedicated thread.
        """
        import_thread = threading.Thread(target=self.init_default_files_s3(), name="Initializer")
        import_thread.start()
