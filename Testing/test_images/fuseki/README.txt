# Docker image providing a local server to run Fuseki integration tests against.
docker run --rm -p 3030:3030 --name fuseki fuseki/latest --mem /ds