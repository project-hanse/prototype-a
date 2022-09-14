echo ""
echo "[INFO] *** Preparing Infrastructure ***"
echo "[INFO] Creating S3 bucket..."

# set signature version for boto3
export AWS_S3_SIGNATURE_VERSION="s3v4"
export MLFLOW_S3_ENDPOINT_URL="${MLFLOW_S3_ENDPOINT_URL}"

# create bucket
aws s3api create-bucket --bucket mlflow-artifacts --region "${AWS_REGION}" --endpoint-url "${MLFLOW_S3_ENDPOINT_URL}"

echo "[INFO] Upgrading database..."
DATABASE_URI=mysql+pymysql://"${MYSQL_USER}":"${MYSQL_PASSWORD}"@"${MYSQL_HOST}":"${MYSQL_PORT}"/"${MYSQL_DATABASE}"
mlflow db upgrade "$DATABASE_URI"

echo ""
echo "[INFO] *** Starting MLflow server ***"
mlflow server --backend-store-uri "$DATABASE_URI" --default-artifact-root s3://mlflow-artifacts --host 0.0.0.0 --port 5005
