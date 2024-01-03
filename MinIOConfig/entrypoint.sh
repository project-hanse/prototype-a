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

# Config follows: https://min.io/docs/minio/container/administration/object-management/transition-objects-to-azure.html
# Setup users for lifecycle management
wget -O - https://min.io/docs/minio/linux/examples/LifecycleManagementAdmin.json | mc admin policy add $MINIO_NAME LifecycleAdminPolicy /dev/stdin
mc admin user add $MINIO_NAME alphaLifecycleAdmin LongRandomSecretKey
mc admin policy set $MINIO_NAME LifecycleAdminPolicy user=alphaLifecycleAdmin

# Add remote storage
#mc admin tier add azure $MINIO_NAME COLDTIER \
#	--endpoint https://hansestoragecoldvm.blob.core.windows.net/vmcoldstorebucket1 \
#	--bucket vmcoldstorebucket1 \
#	--prefix cold \
#	--account-name hansestoragecoldvm \
#	--account-key v1GeH+czMOfjBHS2JMSm3tI6plSEUSE0M2MRYto7pS9upBHBUQ8eL6hIfTxZu6hoicJ36Qu8Bhnu+AStSxJGcw== \
#	--region germanywestcentral

# Remove remote storage
mc admin tier remove azure $MINIO_NAME COLDTIER

# Create transition rules for local buckets (specify when to move data to azure)
mc ilm rm "$MINIO_NAME/datasets" --all --force
mc ilm rm "$MINIO_NAME/plots" --all --force
mc ilm rm "$MINIO_NAME/metadata" --all --force
mc ilm rm "$MINIO_NAME/mlflow-artifacts" --all --force

#mc ilm add "$MINIO_NAME/datasets" \
#	--tier COLDTIER \
#	--transition-days 1
#
#mc ilm add "$MINIO_NAME/plots" \
#	--tier COLDTIER \
#	--transition-days 1
#
#mc ilm add "$MINIO_NAME/metadata" \
#	--tier COLDTIER \
#	--transition-days 1
#
#mc ilm add "$MINIO_NAME/mlflow-artifacts" \
#	--tier COLDTIER \
#	--transition-days 1

# Verify transition rule
#mc ilm ls "$MINIO_NAME/datasets" --transition

echo "Done configuring Object Storage."
