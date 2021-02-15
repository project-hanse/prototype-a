FROM python:3.9-slim
# Alpine isn't used here intentionally because especially with python there are often major issues when compiling dependencies

WORKDIR /src

RUN apt-get update && apt-get install -y build-essential

COPY src/requirements.txt ./

RUN pip install -r /src/requirements.txt

COPY datasets/. /datasets

COPY src/. .

RUN chmod +x ./entrypoint.sh

# The -u flag is important if you want to observe the logs, otherwise python buffers the output
ENTRYPOINT ["./entrypoint.sh"]