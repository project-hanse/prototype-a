#!/bin/bash
echo "Waiting for S3 at address ${S3_HOST}:${S3_PORT}/minio/health/live, attempting every 5s"
sleep 5
until (curl --silent --fail ${S3_HOST}:${S3_PORT}/minio/health/live > /dev/null); do
    printf '.'
    sleep 5
done
echo ' Success: Reached S3'
