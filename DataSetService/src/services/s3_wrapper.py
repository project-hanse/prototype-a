import os
from pathlib import Path

import boto3
from botocore.config import Config
from botocore.exceptions import ClientError

from src.constants.bucket_names import DEFAULT_FILE_BUCKET_NAME
from src.helper.log_helper import LogHelper


class S3Wrapper:
	def __init__(self) -> None:
		super().__init__()
		self.log = LogHelper.get_logger('FileStore')
		self.session = None
		self.s3_client = None
		self.s3_region = None

	def setup(self, s3_endpoint: str, s3_access_key_id: str, s3_access_key_secret: str, s3_region: str):
		self.log.info("Setting up client for S3 service at %s" % s3_endpoint)
		self.log.info("s3_region: %s, key_id: %s, key_secret: %s" % (s3_region, s3_access_key_id, s3_access_key_secret))
		try:
			self.s3_region = s3_region
			self.s3_client = boto3.client(service_name='s3',
																		endpoint_url=s3_endpoint,
																		aws_access_key_id=s3_access_key_id,
																		aws_secret_access_key=s3_access_key_secret,
																		config=Config(signature_version='s3v4'),
																		region_name=s3_region)
		except Exception as e:
			self.log.error("Failed to setup connection to S3 service %s" % str(e))

	def assert_bucket_exists(self, bucket_name: str) -> bool:
		"""
		Checks if a bucket exists and if not, creates it.
		:param bucket_name: name of the bucket to check for
		:return: True if the bucket exists or has been created, False otherwise (in case of an error)
		"""
		self.log.debug("Asserting that file bucket '%s' exists" % bucket_name)
		try:
			buckets = self.s3_client.list_buckets()["Buckets"]
			bucket = next((b for b in buckets if b["Name"] == bucket_name), None)
			if bucket is None:
				self.log.debug(
					"No default bucket for files found, creating bucket '%s'" % bucket_name)
				create_bucket_response = self.s3_client.create_bucket(
					Bucket=bucket_name,
					CreateBucketConfiguration={
						'LocationConstraint': self.s3_region,
					}
				)
				self.log.info("Created new default file bucket %s" % create_bucket_response)
			else:
				self.log.info("Default file bucket '%s' exists since %s" % (bucket['Name'], bucket['CreationDate']))
			return True
		except ClientError as e:
			self.log.error(e)
			return False
		except Exception as e:
			self.log.error('Unexpected error while asserting default file bucket exists %s' % e)
			return False

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
		if self.key_exists(filename, bucket_name):
			self.log.info("Object with key '%s' already exists in bucket %s" % (filename, bucket_name))
			return False
		with open(resolved_path, 'rb') as data:
			self.s3_client.upload_fileobj(data, bucket_name, filename)
		self.log.info("Uploaded file '%s' to bucket '%s'" % (filename, bucket_name))
		return True

	def key_exists(self, key: str, bucket_name: str) -> bool:
		"""
		Checks if an object (file) with a given key (name) exists in the default file bucket.
		:param key: name of the object to check for
		:param bucket_name: name of the bucket to check in
		:return: True if the object exists, False otherwise
    """

		self.log.debug("Checking if key exists in bucket %s" % bucket_name)
		try:
			self.s3_client.head_object(Bucket=bucket_name, Key=key)
			self.log.debug("Key '%s' exists in bucket %s" % (key, bucket_name))
			return True
		except ClientError as e:
			if e.response['ResponseMetadata']['HTTPStatusCode'] == 404:
				self.log.debug("Key '%s' does not exist in bucket %s" % (key, bucket_name))
				return False
			raise e
