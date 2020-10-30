#!/bin/bash

# Exit on first error
set -e

service virtuoso-opensource-7 start

function isql {
 /usr/local/bin/isql localhost dba dba VERBOSE=OFF BANNER=OFF PROMPT=OFF ECHO=OFF BLOBS=ON ERRORS=stdout "$@"
}


# FIXME: Is this needed? It seems overly permissive..
  echo "Configuring SPARQL"
  isql 'exec=GRANT EXECUTE ON DB.DBA.SPARQL_INSERT_DICT_CONTENT TO "SPARQL";'
  isql 'exec=GRANT EXECUTE ON DB.DBA.L_O_LOOK TO "SPARQL";'
  isql 'exec=GRANT EXECUTE ON DB.DBA.SPARUL_RUN TO "SPARQL";'
  isql 'exec=GRANT EXECUTE ON DB.DBA.SPARQL_DELETE_DICT_CONTENT TO "SPARQL";'
  isql 'exec=GRANT EXECUTE ON DB.DBA.RDF_OBJ_ADD_KEYWORD_FOR_GRAPH TO "SPARQL";'

echo "Populating from /staging/staging.sql"
isql /staging/staging.sql 

# How many cores? Get last processor number
cores=$(($(cat /proc/cpuinfo  | grep "^processor" | tail -1 | sed 's/.*: //')+1))

# > it is recommended a maximum of no cores / 2.5, to optimally 
# > parallelize the data load and hence maximize load speed
# http://virtuoso.openlinksw.com/dataspace/doc/dav/wiki/Main/VirtBulkRDFLoader
# 
# .. but we'll not do more than 8 anyway, as that would generally kill I/O
MAX_LOADERS=8
loaders=$(($cores*2/5 + 1))
loaders=$((loaders<MAX_LOADERS?loaders:MAX_LOADERS))
# http://stackoverflow.com/questions/10415064/how-to-calculate-the-minimum-of-two-variables-simply-in-bash

echo "Starting $loaders rdf_loader_runs"

for loader in `seq 1 8`; do
  echo Starting RDF loader $loader 
  isql 'EXEC=rdf_loader_run();' & 
done
wait
echo "Checkpointing"
isql 'EXEC=checkpoint;' 
echo -n "Staging finished, total triples: " 
isql 'EXEC=SPARQL SELECT COUNT(*) WHERE { ?s ?p ?o} ;'

service virtuoso-opensource-7 stop

