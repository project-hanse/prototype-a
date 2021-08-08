import boto3
from botocore.exceptions import ClientError

from src.helper.log_helper import LogHelper


class FileStore:
    default_file_bucket_name = "defaultfiles"

    def __init__(self) -> None:
        super().__init__()
        self.log = self.logger = LogHelper.get_logger('FileStore')
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

    def assert_bucket_exists(self):
        self.log.debug("Asserting that default file bucket '%s' exists" % self.default_file_bucket_name)
        try:
            buckets = self.s3_client.list_buckets()["Buckets"]
            default_file_bucket = next((b for b in buckets if b["Name"] == self.default_file_bucket_name), None)
            if default_file_bucket is None:
                self.log.debug(
                    "No default bucket for files found, creating bucket '%s'" % self.default_file_bucket_name)
                create_bucket_response = self.s3_client.create_bucket(
                    Bucket=self.default_file_bucket_name,
                    CreateBucketConfiguration={
                        'LocationConstraint': 'eu-west-1',
                    }
                )
                self.log.info("Created new default file bucket %s" % create_bucket_response)
            else:
                self.log.info("Default file bucket '%s' exists since %s" % (
                    default_file_bucket['Name'], default_file_bucket['CreationDate']))

        except ClientError as e:
            self.log.error(e)
        except Exception as e:
            self.log.error('Unexpected error while asserting default file bucket exists %s' % e)
