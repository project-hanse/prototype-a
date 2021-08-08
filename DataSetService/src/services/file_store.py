import os
from pathlib import Path

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
                        'LocationConstraint': 'eu-west-2',
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

    def store_file_to_bucket(self, file_path: str):
        resolved_path = str(Path(file_path).resolve())
        filename = os.path.basename(resolved_path)

        if not os.path.isfile(resolved_path):
            self.log.error("File %s does not exist", resolved_path)
            return

        self.log.info("Uploading file '%s' to bucket '%s'" % (filename, self.default_file_bucket_name))
        # TODO: extend this check to include the content-md5 hash to allow newer versions of files
        if self.key_exists(filename):
            self.log.debug("object with key '%s' already exists" % filename)
            return
        with open(resolved_path, 'rb') as data:
            self.s3_client.upload_fileobj(data, self.default_file_bucket_name, filename)

    def key_exists(self, key: str) -> bool:
        """
        Checks if an object (file) with a given key (name) exists in the default file bucket.
        """
        try:
            response = self.s3_client.head_object(Bucket=self.default_file_bucket_name, Key=key)
            self.log.debug("Checking if key exists responded with %s" % response)
            return True
        except ClientError as e:
            if e.response['ResponseMetadata']['HTTPStatusCode'] == 404:
                return False
            raise e
