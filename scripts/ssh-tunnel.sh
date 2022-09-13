#!/bin/bash
echo "Starting SSH tunnel..."
ssh -L 8000:128.131.58.71:80 user@128.131.58.71
