FROM python:3.10.6-slim

RUN apt-get update && apt-get install -y build-essential curl

WORKDIR /app

COPY requirements.txt .

RUN pip3 install -r requirements.txt

COPY entrypoint.sh .
COPY wait-for-it.sh .

RUN chmod +x ./*.sh

RUN useradd mlflowuser

USER mlflowuser

CMD ./wait-for-it.sh && ./entrypoint.sh

