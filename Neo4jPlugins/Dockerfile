FROM maven:3.8-openjdk-11-slim AS build-stage

WORKDIR /build

COPY . .

RUN mvn -B --no-transfer-progress clean package

FROM neo4j:4.3
MAINTAINER "Konstantin Strümpf <k.struempf@icloud.com>"

COPY --from=build-stage /build/target/hanse-procedures-* "${NEO4J_HOME}/plugins"
