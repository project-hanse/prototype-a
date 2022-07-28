"""
The path to the directory where local copies of OpenML datasets are stored.
This directory can be shared by multiple instances of the operation worker.
"""
import os

open_ml_datasets_local_copy = os.getenv("OPENML_LOCAL_COPY_DIR", "/var/lib/operation-worker/data/openml/local-copy")
