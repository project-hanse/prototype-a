import threading

import boto3
from botocore.exceptions import ClientError


class InitService:

    def __init__(self) -> None:
        super().__init__()
        self.session = None
        self.s3_client = None

    def setup(self, s3_endpoint: str):
        print("Setting up client for S3 service at %s" % s3_endpoint)
        try:
            self.session = boto3.session.Session()
            self.s3_client = self.session.client(
                service_name='s3',
                aws_access_key_id='aaa',
                aws_secret_access_key='bbb',
                endpoint_url=s3_endpoint
            )
        except Exception as e:
            print("Failed to setup connection to S3 service %s" % str(e))

    def init_default_files_s3(self):
        """
        Makes sure that a number of default files (.csv, .xlsx) are available in the S3 file store.
        """
        print("Initializing files in S3 bucket")
        try:
            response = self.s3_client.list_buckets()
            print("Available S3 buckets: %s" % response['Buckets'])
        except ClientError as e:
            print(e)

    def init_default_files_s3_in_background(self):
        """
        Initializes the default data files in a dedicated thread.
        """
        import_thread = threading.Thread(target=self.init_default_files_s3(), name="Initializer")
        import_thread.start()
