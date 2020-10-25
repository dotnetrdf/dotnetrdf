# Basic Fuseki Dockerfile.

## To do:
# VOLUME and databases

FROM java:8-jdk

LABEL maintainer="The Apache Jena community <users@jena.apache.org>"

ARG VERSION=3.16.0

ENV URL=https://repository.apache.org/content/repositories/releases/org/apache/jena/jena-fuseki-server/${VERSION}/jena-fuseki-server-${VERSION}.jar
ENV BASE=/mnt/apache-fuseki

## VOLUME /mnt/

RUN mkdir -p $BASE

WORKDIR $BASE

RUN curl --silent --show-error --output fuseki-server.jar $URL

EXPOSE 3030

ENTRYPOINT [ "/usr/bin/java", "-jar", "fuseki-server.jar" ]