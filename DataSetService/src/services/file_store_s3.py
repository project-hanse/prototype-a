import os
from pathlib import Path

import boto3
from botocore.config import Config

from src.constants.bucket_names import DEFAULT_FILE_BUCKET_NAME
from src.helper.log_helper import LogHelper
from src.helper.s3_helper import assert_bucket_exists, key_exists


class FileStoreS3:
	def __init__(self) -> None:
		super().__init__()
		self.log = LogHelper.get_logger('FileStore')
		self.session = None
		self.s3_client = None
		self.s3_region = None

	def setup(self, s3_endpoint: str, s3_access_key_id: str, s3_access_key_secret: str, s3_region: str):
		self.log.info("Setting up S3 file store (s3_endpoint: %s)" % s3_endpoint)
		try:
			self.s3_region = s3_region
			self.s3_client = boto3.client(service_name='s3',
																		endpoint_url=s3_endpoint,
																		aws_access_key_id=s3_access_key_id,
																		aws_secret_access_key=s3_access_key_secret,
																		config=Config(signature_version='s3v4'),
																		region_name=s3_region)
			assert_bucket_exists(self.log, self.s3_client, self.s3_region, DEFAULT_FILE_BUCKET_NAME)
		except Exception as e:
			self.log.error("Failed to setup connection to S3 service %s" % str(e))

	def store_file_to_bucket(self, file_path: str, bucket_name: str = None) -> bool:
		"""
		Stores a file to the default file bucket.
		:param file_path: name of the file to store
		:param bucket_name: name of the bucket to store in, defaults to the default file bucket
		:return: True if the file was stored, False otherwise
		"""
		if bucket_name is None:
			bucket_name = DEFAULT_FILE_BUCKET_NAME

		resolved_path = str(Path(file_path).resolve())
		filename = os.path.basename(resolved_path)

		if not os.path.isfile(resolved_path):
			self.log.error("File %s does not exist", resolved_path)
			return False

		self.log.debug("Uploading file '%s' to bucket '%s'" % (filename, bucket_name))
		# TODO: extend this check to include the content-md5 hash to allow newer versions of files
		if key_exists(self.log, self.s3_client, self.s3_client, filename, bucket_name):
			self.log.info("Object with key '%s' already exists in bucket %s" % (filename, bucket_name))
			return False
		with open(resolved_path, 'rb') as data:
			self.s3_client.upload_fileobj(data, bucket_name, filename)
		self.log.info("Uploaded file '%s' to bucket '%s'" % (filename, bucket_name))
		return True
