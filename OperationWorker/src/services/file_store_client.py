import logging
from typing import Optional

import boto3
import botocore
import chardet
from botocore.exceptions import ClientError

from src.helper.operations_helper import OperationsHelper
from src.models.dataset import Dataset


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
		if detection["encoding"] is None:
			self.log.info('Could not detect encoding of file')
			return None
		self.log.info('Detected charset %s' % detection["encoding"])
		return body.decode(detection["encoding"])

	def get_object_content_as_binary(self, input_object_bucket: str, input_object_key: str):
		self.log.info("Loading object content from bucket '%s' for object with key '%s'"
									% (input_object_bucket, input_object_key))

		# Implemented based on:
		# https://stackoverflow.com/questions/35803601/reading-a-file-from-a-private-s3-bucket-to-a-pandas-dataframe/43838676
		if self.s3_client is None:
			raise Exception('Service not initialized')

		response = self.s3_client.get_object(Bucket=input_object_bucket, Key=input_object_key)
		if response is None:
			return None

		return response['Body'].read()

	def create_bucket_if_not_exists(self, bucket_name: str):
		self.log.info("Creating bucket '%s'" % bucket_name)
		if self.s3_client is None:
			raise Exception('Service not initialized')
		try:
			response = self.s3_client.create_bucket(Bucket=bucket_name)
			self.log.debug("Created bucket '%s' (response: %s)" % (bucket_name, str(response)))
		except ClientError as e:
			if e.response['Error']['Code'] == 'BucketAlreadyOwnedByYou':
				self.log.debug("Bucket '%s' already exists" % bucket_name)
			else:
				logging.error(e)
				return False

	def store_file(self, dataset: Dataset) -> bool:
		self.log.info("Storing file '%s' to bucket '%s'" % (dataset.get_key(), dataset.get_store()))
		if self.s3_client is None:
			raise Exception('Service not initialized')
		self.create_bucket_if_not_exists(dataset.get_store())
		temp_path = OperationsHelper.get_temporary_file_path(dataset)
		try:
			response = self.s3_client.upload_file(temp_path, dataset.get_store(), dataset.get_key())
			self.log.debug(
				"Uploaded file '%s' to bucket '%s' (response: %s)" % (temp_path, dataset.get_store(), str(response)))
		except botocore.exceptions.ClientError as err:
			status = err.response["ResponseMetadata"]["HTTPStatusCode"]
			errcode = err.response["Error"]["Code"]
			if status == 404:
				self.log.warning("Missing object, %s", errcode)
			elif status == 403:
				self.log.error("Access denied, %s", errcode)
			else:
				self.log.exception("Error in request, %s", errcode)
			return False
		return True
