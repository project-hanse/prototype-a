from logging import Logger

from botocore.client import BaseClient
from botocore.exceptions import ClientError


def assert_bucket_exists(logger: Logger, s3_client: BaseClient, s3_region: str, bucket_name: str) -> bool:
	"""
	Checks if a bucket exists and if not, creates it.
	:param logger: logger to use for logging
	:param s3_client: client to use for checking and creating the bucket
	:param s3_region: region of the bucket
	:param bucket_name: name of the bucket to check for
	:return: True if the bucket exists or has been created, False otherwise (in case of an error)
	"""
	logger.debug("Asserting that bucket '%s' exists" % bucket_name)
	try:
		buckets = s3_client.list_buckets()["Buckets"]
		bucket = next((b for b in buckets if b["Name"] == bucket_name), None)
		if bucket is None:
			logger.debug(
				"No default bucket for files found, creating bucket '%s'" % bucket_name)
			create_bucket_response = s3_client.create_bucket(
				Bucket=bucket_name,
				CreateBucketConfiguration={
					'LocationConstraint': s3_region,
				}
			)
			logger.info("Created new default file bucket %s" % create_bucket_response)
		else:
			logger.info("Default file bucket '%s' exists since %s" % (bucket['Name'], bucket['CreationDate']))
		return True
	except ClientError as e:
		logger.error(e)
		return False
	except Exception as e:
		logger.error('Unexpected error while asserting default file bucket exists %s' % e)
		return False


def key_exists(logger: Logger, s3_client: BaseClient, s3_region: str, key: str, bucket_name: str) -> bool:
	"""
	Checks if an object (file) with a given key (name) exists in the default file bucket.
	:param key: name of the object to check for
	:param bucket_name: name of the bucket to check in
	:return: True if the object exists, False otherwise
	"""

	logger.debug("Checking if key exists in bucket %s" % bucket_name)
	try:
		s3_client.head_object(Bucket=bucket_name, Key=key)
		logger.debug("Key '%s' exists in bucket %s" % (key, bucket_name))
		return True
	except ClientError as e:
		if e.response['ResponseMetadata']['HTTPStatusCode'] == 404:
			logger.debug("Key '%s' does not exist in bucket %s" % (key, bucket_name))
			return False
		raise e
