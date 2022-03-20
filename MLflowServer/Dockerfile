FROM python:3.7-slim-buster

WORKDIR /app

RUN apt-get update && apt-get install -y build-essential curl

COPY requirements.txt .

RUN pip3 install -r requirements.txt

COPY entrypoint.sh .
COPY wait-for-it.sh .

RUN chmod +x ./*.sh

CMD ./wait-for-it.sh && ./entrypoint.sh

