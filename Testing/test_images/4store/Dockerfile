FROM debian:stretch-slim

MAINTAINER Kal Ahmed <techquila@gmail.com>

RUN apt-get update && apt-get install -y \
            git \
            make \
            autoconf \
            libtool \
	    pkg-config \ 
	    libncurses5-dev \ 
	    libreadline6-dev \
	    zlib1g-dev \
	    uuid-dev \
	    libxml2-dev \
	    libglib2.0-dev \ 
	    libraptor2-dev \ 
	    librasqal3-dev \ 
    && apt-get clean \ 
    && rm -rf /var/lib/apt/lists/*

# Install 4store and clean up
RUN git clone https://github.com/garlik/4store.git \
    && cd 4store \
    && git checkout v1.1.6 \
    && ./autogen.sh \
    && ./configure \
    && make \
    && make install \
    && make clean \
    && cd .. \
    && rm -rf 4store

# create directory for 4store logs
RUN mkdir /var/log/4store

EXPOSE 8080

# Run a test store on port 8080
CMD 4s-backend-setup store1 \
    && 4s-backend store1 \
    && 4s-httpd -D -U -d store1
