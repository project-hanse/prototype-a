echo "Creating S3 bucket..."
curl -X PUT -u "$AWS_ACCESS_KEY_ID":"$AWS_SECRET_ACCESS_KEY" "${MLFLOW_S3_ENDPOINT_URL}/mlflow-artifacts"

echo ""
echo "Starting MLflow server..."
mlflow server --backend-store-uri mysql+pymysql://"${MYSQL_USER}":"${MYSQL_PASSWORD}"@"${MYSQL_HOST}":"${MYSQL_PORT}"/"${MYSQL_DATABASE}" --default-artifact-root s3://mlflow-artifacts --host 0.0.0.0 --port 5005
