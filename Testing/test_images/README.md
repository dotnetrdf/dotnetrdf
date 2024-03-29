# Test Docker Image Files

This directory contains Dockefiles to build images to use for integration testing of dotNetRDF connectors. The images are designed
only for testing purposes and are in no way good examples of using these stores in a production environment!

## Images Provided

### Fuseki
    docker run --rm -p 3030:3030 --name fuseki fuseki:latest --mem /ds
    
Exposes a store at http://localhost:3030/ds/data

### 4Store

    docker run --rm -p 8080:8080 --name 4store 4store
    
Exposes a store at http://localhost:8080

### Sesame/RDF4J

    docker run --rm -p 8081:8080 --name rdf4j rdf4j
    
Exposes an RDF4J server at http://localhost:8080/rdf4j-server with a single in-memory repository named `unit-test`

### Allegrograph

The Dockerfile provided just inserts a default configuration file into the Allegrograph image provided by Franz Inc.

Note that this image requires a increased shared memory setting to run.

    docker run --rm -p 10000-10035:10000-10035 --shm-size 1g --name agraph allegrograph
    
Exposes an Allegrograph server at http://localhost:10035/ with a catalog named `test` accessible with user name `test` and password `test`. This catalog is empty on initialisation and test code must ensure that the required repository is created as needed.

### Stardog

This image requires a license file to run. You can obtain a free license file from https://www.stardog.com/. The image itself is the base Stardog Docker image with no modifications and the local `stardog` directory here is just provided as a default location for the Stardog home volume. If you place the license file you receive from Stardog into the `stardog` directory and then start the service (`docker compose up stardog`) the server should automatically find and use that license file. Please note that as this is the home directory for the server, running the server will also create all of the Stardog database files and log files in this directory.

### Virtuoso

The build for this image uses the Virtuoso Open Source edition. The initial image build time is quite lengthy as it builds from source code.

     docker run --rm -p 8890:8890 -p 1111:1111 --name virtuoso virtuoso

