FROM ghcr.io/mlflow/mlflow:v2.10.2

RUN apt-get update && apt-get install -y build-essential curl awscli

WORKDIR /app

COPY requirements.txt .

RUN pip3 install -r requirements.txt

COPY entrypoint.sh .
COPY wait-for-it.sh .

RUN chmod +x ./*.sh

RUN useradd mlflowuser

USER mlflowuser

CMD ./wait-for-it.sh && ./entrypoint.sh

