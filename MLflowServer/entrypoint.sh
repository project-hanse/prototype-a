echo "Creating S3 bucket..."
curl -X PUT -u "$AWS_ACCESS_KEY_ID":"$AWS_SECRET_ACCESS_KEY" "${MLFLOW_S3_ENDPOINT_URL}/mlflow-artifacts"

DATABASE_URI=mysql+pymysql://"${MYSQL_USER}":"${MYSQL_PASSWORD}"@"${MYSQL_HOST}":"${MYSQL_PORT}"/"${MYSQL_DATABASE}"
mlflow db upgrade "$DATABASE_URI"

echo ""
echo "Starting MLflow server..."
mlflow server --backend-store-uri "$DATABASE_URI" --default-artifact-root s3://mlflow-artifacts --host 0.0.0.0 --port 5005
