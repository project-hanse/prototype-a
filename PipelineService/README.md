# Pipeline Service

A webservice for storing and executing pipelines.

## Configuration

Configuration parameters via environment variables or `appsettings.json` file.

| Variable name | Description | Default | Example |
| ------ | ------ | ------ | ------ |
| `MQTT_HOST` | The MQTT broker's hostname for a TCP based connection. | `message-broker` | `broker.hivemq.com` |
| `MQTT_PORT` | The MQTT broker's port for a TCP based connection. | `1883` | `1883` |
| `MQTT_CLIENT_ID` | The client id used for connecting to the MQTT broker. | `PipelineService-[RANDOM-GUID]` | `PipelineService-1` |
| `MQTT_USER` | The user name used for authenticating to the MQTT broker. | - | `username` |
| `MQTT_PASSWORD` | The password used for authenticating to the MQTT broker. | - | `I1X9jWbCzg6Z` |