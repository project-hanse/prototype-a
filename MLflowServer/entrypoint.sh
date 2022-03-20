echo "Starting MLflow server..."

mlflow server --backend-store-uri mysql+pymysql://"${MYSQL_USER}":"${MYSQL_PASSWORD}"@"${MYSQL_HOST}":"${MYSQL_PORT}"/"${MYSQL_DATABASE}" --default-artifact-root s3://"${S3_HOST}":"${S3_PORT}"/mlflow/ --host 0.0.0.0
