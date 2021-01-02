# Block Worker

A block worker is a program that executes an operation (
eg. [data frame concatenation](https://pandas.pydata.org/pandas-docs/stable/user_guide/merging.html)
, [data cleanup](https://pandas.pydata.org/pandas-docs/stable/user_guide/missing_data.html)) on one or more datasets and
produces one ore more datasets. It is written in Python using
the [Pandas](https://pandas.pydata.org/pandas-docs/stable/index.html) framework.

## Configuration

Configuration parameters via environment variables.

| Variable name | Description | Default | Example |
| ------ | ------ | ------ | ------ |
| `MQTT_HOST` | The MQTT broker's hostname for a TCP based connection. | `message-broker` | `broker.hivemq.com` |
| `MQTT_PORT` | The MQTT broker's port for a TCP based connection. | `1883` | `1883` |
| `MQTT_CLIENT_ID` | The client id used for connecting to the MQTT broker. | `BlockWorker-[RANDOM-GUID]` | `BlockWorker-1` |
| `MQTT_TOPIC_SUB` | The topic the worker subscribes to for messages (blocks that will be executed). | `execute/+` | `BlockWorker-1` |