import logging
from typing import Optional

import boto3
import chardet


class FileStoreClient:

    def __init__(self, logger: logging) -> None:
        super().__init__()
        self.log = logger
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
                endpoint_url=s3_endpoint)
            if self.s3_client is None:
                self.log.warning("Could not create client for S3 file store")
                return False
        except Exception as e:
            self.log.error("Failed to setup connection to S3 service %s" % str(e))
        return True

    def get_object_content(self, input_object_bucket: str, input_object_key: str) -> Optional[str]:
        self.log.info("Loading object content from bucket '%s' for object with key '%s'"
                      % (input_object_bucket, input_object_key))

        # Implemented based on:
        # https://stackoverflow.com/questions/35803601/reading-a-file-from-a-private-s3-bucket-to-a-pandas-dataframe/43838676
        if self.s3_client is None:
            raise Exception('Service not initialized')

        response = self.s3_client.get_object(Bucket=input_object_bucket, Key=input_object_key)
        if response is None:
            return None

        body = response['Body'].read()
        detection = chardet.detect(body)
        self.log.info('Detected charset %s' % detection["encoding"])
        return body.decode(detection["encoding"])
