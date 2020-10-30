FROM tomcat:8.5-jre8-alpine

ENV JAVA_OPTS="-Xmx2g"
ENV CATALINA_OPTS="-Dorg.eclipse.rdf4j.appdata.basedir=/var/rdf4j"
ARG VERSION=3.4.3

ENV URL=http://download.eclipse.org/rdf4j/eclipse-rdf4j-${VERSION}-sdk.zip
RUN adduser -S tomcat

WORKDIR /tmp
RUN wget -q -O rdf4j.zip $URL
RUN	unzip -q /tmp/rdf4j.zip
RUN	rm -rf /usr/local/tomcat/webapps/*
RUN	cp /tmp/eclipse-rdf4j*/war/*.war /usr/local/tomcat/webapps
RUN	rm -rf /tmp/eclipse
RUN	rm /tmp/rdf4j.zip
# RUN	mkdir -p /var/rdf4j
RUN mkdir -p /var/rdf4j/server/repositories/unit-test
COPY config.ttl /var/rdf4j/server/repositories/unit-test/config.ttl
RUN	chown -R tomcat /var/rdf4j /usr/local/tomcat
RUN	chmod a+x /usr/local/tomcat /usr/local/tomcat/bin /usr/local/tomcat/bin/catalina.sh

COPY web.xml /usr/local/tomcat/conf/web.xml
USER tomcat

WORKDIR /usr/local/tomcat/

# never got this syntax to work with docker-compose
#VOLUME /var/rdf4j
#VOLUME /usr/local/tomcat/logs

EXPOSE 8080