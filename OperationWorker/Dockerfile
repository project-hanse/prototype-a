FROM ubuntu:20.04 AS builder-image
# Based on https://luis-sena.medium.com/creating-the-perfect-python-dockerfile-51bdec41f1c8

# avoid stuck build due to user prompt
ARG DEBIAN_FRONTEND=noninteractive

RUN apt-get update && apt-get install --no-install-recommends -y python3.9 python3.9-dev python3.9-venv python3-pip python3-wheel build-essential && \
	apt-get clean && rm -rf /var/lib/apt/lists/*

# create and activate virtual environment
# using final folder name to avoid path issues with packages
RUN python3.9 -m venv /home/workeruser/venv
ENV PATH="/home/workeruser/venv/bin:$PATH"

# install requirements
COPY requirements.txt .
RUN pip3 install wheel
RUN pip3 install -r requirements.txt

# Runner
FROM ubuntu:20.04 AS runner-image
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

RUN apt-get update && apt-get install --no-install-recommends -y python3.9 python3-venv && \
	apt-get clean && rm -rf /var/lib/apt/lists/*
# RUN useradd --create-home workeruser
COPY --from=builder-image /home/workeruser/venv /home/workeruser/venv

# USER workeruser
RUN mkdir /home/workeruser/code
WORKDIR /home/workeruser/code

COPY setup.py .

COPY src ./src

# make sure all messages always reach console
ENV PYTHONUNBUFFERED=1

# activate virtual environment
ENV VIRTUAL_ENV=/home/workeruser/venv
ENV PATH="/home/workeruser/venv/bin:$PATH"

RUN pip install .
CMD ["python", "-u", "./src/main.py"]
