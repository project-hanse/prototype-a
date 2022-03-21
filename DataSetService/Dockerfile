FROM python:3.10.3-slim
# Alpine isn't used here intentionally because especially with python there are often major issues when compiling dependencies
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

WORKDIR /app

RUN apt-get update && apt-get install -y build-essential curl

COPY requirements.txt .

RUN pip install -r ./requirements.txt

COPY setup.py .
COPY entrypoint.sh .
COPY wait-for-it.sh .
COPY datasets ./datasets

COPY src ./src

RUN chmod +x ./*.sh

CMD ./wait-for-it.sh && ./entrypoint.sh
