FROM python:3.9-slim
# Alpine isn't used here intentionally because especially with python there are often major issues when compiling dependencies

WORKDIR /app

# RUN apt-get update && apt-get install -y build-essential

COPY requirements.txt .

RUN pip install -r ./requirements.txt

COPY setup.py .
COPY entrypoint.sh .

COPY src ./src

RUN chmod +x ./entrypoint.sh

# The -u flag is important if you want to observe the logs, otherwise python buffers the output
ENTRYPOINT ["./entrypoint.sh"]