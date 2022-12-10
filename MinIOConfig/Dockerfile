FROM ubuntu:22.04

RUN apt-get update && apt-get install -y curl && apt-get install -y wget

COPY entrypoint.sh .

RUN chmod +x ./entrypoint.sh

CMD ./entrypoint.sh

