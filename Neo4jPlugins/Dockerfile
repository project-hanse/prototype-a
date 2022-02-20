FROM maven:3.8-openjdk-11-slim AS build-stage

WORKDIR /build

COPY . .

RUN mvn -B --no-transfer-progress clean package

FROM neo4j:4.4.4
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

COPY ./scripts "$NEO4J_HOME"/scripts

RUN chmod +x "$NEO4J_HOME"/scripts/*.sh

COPY --from=build-stage /build/target/hanse-procedures-* "${NEO4J_HOME}/plugins"

ENV EXTENSION_SCRIPT="${NEO4J_HOME}/scripts/neo4j_config_extension.sh"
