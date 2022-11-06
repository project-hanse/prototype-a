FROM maven:3.8-openjdk-11-slim AS build-stage

WORKDIR /build

COPY . .

RUN mvn -B --no-transfer-progress clean package

FROM neo4j:5.1.0
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

# install curl
RUN apt-get update && apt-get install -y curl

ENV NEO4J_apoc_export_file_enabled=true
ENV NEO4JLABS_PLUGINS=\[\"apoc\",\"graph-data-science\"\]

COPY ./scripts "$NEO4J_HOME"/scripts

RUN chmod +x "$NEO4J_HOME"/scripts/*.sh

COPY --from=build-stage /build/target/hanse-procedures-* "${NEO4J_HOME}/plugins"

ENV EXTENSION_SCRIPT="${NEO4J_HOME}/scripts/neo4j_config_extension.sh"
