#!/bin/bash
echo "Waiting for S3 at address ${MLFLOW_S3_ENDPOINT_URL}/health, attempting every 5s"
sleep 5
until $(curl --silent --fail ${MLFLOW_S3_ENDPOINT_URL}/health | grep "\"s3\": \"running\"" > /dev/null); do
    printf '.'
    sleep 5
done
echo ' Success: Reached S3'
