#!/bin/bash
echo "Waiting for S3 at address ${MLFLOW_S3_ENDPOINT_URL}/minio/health/live, attempting every 5s"
sleep 5
until (curl --silent --fail "${MLFLOW_S3_ENDPOINT_URL}/minio/health/live" >/dev/null); do
	printf '.'
	sleep 5
done
echo " Success: Reached S3"
echo ""
