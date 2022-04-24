FROM ubuntu:14.04

ENV BUILD_DEPS wget
ENV URL https://github.com/openlink/virtuoso-opensource/releases/download/v7.2.6.1/virtuoso-opensource.x86_64-generic_glibc25-linux-gnu.tar.gz

RUN apt-get update
RUN apt-get install $BUILD_DEPS

RUN wget --no-check-certificate --quiet $URL -O /tmp/virtuoso-opensource.tar.gz
RUN tar xf /tmp/virtuoso-opensource.tar.gz
WORKDIR virtuoso-opensource

RUN ln -s /virtuoso-opensource/bin/isql /usr/local/bin/isql

# Make a usable copy of the sample ini file
RUN mkdir /virtuoso-opensource/config
RUN cp /virtuoso-opensource/database/virtuoso.ini.sample /virtuoso-opensource/config/virtuoso.ini

# Enable mountable /virtuoso for data storage, which
# we'll symlink the standard db folder to point to
RUN mkdir /virtuoso
VOLUME /virtuoso
RUN rm -rf /virtuoso-opensource/database
RUN ln -s /virtuoso /virtuoso-opensource/database

# /staging for loading data
RUN mkdir /staging ; sed -i '/DirsAllowed/ s:$:,/staging:' /virtuoso-opensource/config/virtuoso.ini
VOLUME /staging

COPY staging.sh /usr/local/bin/
COPY docker-entrypoint.sh /
RUN chmod 755 /usr/local/bin/staging.sh /docker-entrypoint.sh


# Virtuoso ports
EXPOSE 8890
EXPOSE 1111
WORKDIR /virtuoso-opensource/bin
# Modify config-file on start-up to reflect memory available
ENTRYPOINT ["/docker-entrypoint.sh"]
# Run virtuoso in the foreground
CMD ["/virtuoso-opensource/bin/virtuoso-t", "+wait", "+foreground", "+configfile", "/virtuoso-opensource/config/virtuoso.ini"]