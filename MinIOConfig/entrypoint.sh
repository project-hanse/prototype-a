echo "Installing MinIO Client..."

curl https://dl.min.io/client/mc/release/linux-amd64/mc \
	--create-dirs \
	-o $HOME/minio-binaries/mc

chmod +x $HOME/minio-binaries/mc
export PATH=$PATH:$HOME/minio-binaries/
export MINIO_NAME="myminio"

echo "Connecting to MinIO Server ($MINIO_HOSTNAME)..."
bash +o history
mc alias set "$MINIO_NAME" "$MINIO_HOSTNAME" "$MINIO_ACCESS_KEY" "$MINIO_SECRET_KEY"
bash -o history

echo "Testing connection to MinIO Server..."
mc admin info "$MINIO_NAME"
mc ls "$MINIO_NAME"
echo ""

echo "Starting configuration of Object Storage..."



echo "Done configuring Object Storage."
