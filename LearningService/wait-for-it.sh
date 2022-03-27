#!/bin/bash
echo "Waiting for S3 at address ${S3_HOST}:${S3_PORT}/health, attempting every 5s"
sleep 5
until $(curl --silent --fail ${S3_HOST}:${S3_PORT}/health | grep "\"s3\": \"running\"" > /dev/null); do
    printf '.'
    sleep 5
done
echo " Success: Reached S3"
