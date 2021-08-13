#!/bin/bash
pwd
ls -l
pip install .
exec python3 -u ./src/app.py
