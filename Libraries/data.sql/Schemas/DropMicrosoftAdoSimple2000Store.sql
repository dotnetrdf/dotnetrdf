-- Drop the Tables

DROP TABLE QUADS;
DROP TABLE GRAPHS;
DROP TABLE NODES;
DROP TABLE BNODES;

-- Drop the Stored Procedures

DROP PROCEDURE GetVersion;
DROP PROCEDURE GetSchemaName;

DROP PROCEDURE ClearStore;
DROP PROCEDURE ClearStoreFull;

DROP PROCEDURE GetGraphID;
DROP PROCEDURE GetOrCreateGraphID;
DROP PROCEDURE GetGraphUri;
DROP PROCEDURE GetGraphUris;

DROP PROCEDURE ClearGraph;
DROP PROCEDURE ClearGraphForOverwrite;
DROP PROCEDURE ClearGraphByUri;
DROP PROCEDURE ClearGraphForOverwriteByUri;

DROP PROCEDURE DeleteGraph;
DROP PROCEDURE DeleteGraphByUri;

DROP PROCEDURE GetNodeID;
DROP PROCEDURE GetOrCreateNodeID;
DROP PROCEDURE CreateBlankNodeID;
DROP PROCEDURE GetNodeData;

DROP PROCEDURE HasQuad;
DROP PROCEDURE HasQuadData;
DROP PROCEDURE AssertQuad;
DROP PROCEDURE AssertQuadData;
DROP PROCEDURE RetractQuad;
DROP PROCEDURE RetractQuadData;

DROP PROCEDURE GetQuads;
DROP PROCEDURE GetQuadsVirtual;
DROP PROCEDURE GetQuadsData;

DROP PROCEDURE GetGraphQuads;
DROP PROCEDURE GetGraphQuadsVirtual;
DROP PROCEDURE GetGraphQuadsData;

DROP PROCEDURE GetQuadsWithSubject;
DROP PROCEDURE GetQuadsWithSubjectVirtual;
DROP PROCEDURE GetQuadsWithSubjectData;
DROP PROCEDURE GetQuadsWithPredicate;
DROP PROCEDURE GetQuadsWithPredicateVirtual;
DROP PROCEDURE GetQuadsWithPredicateData;
DROP PROCEDURE GetQuadsWithObject;
DROP PROCEDURE GetQuadsWithObjectVirtual;
DROP PROCEDURE GetQuadsWithObjectData;
DROP PROCEDURE GetQuadsWithSubjectPredicate;
DROP PROCEDURE GetQuadsWithSubjectPredicateVirtual;
DROP PROCEDURE GetQuadsWithSubjectPredicateData;
DROP PROCEDURE GetQuadsWithSubjectObject;
DROP PROCEDURE GetQuadsWithSubjectObjectVirtual;
DROP PROCEDURE GetQuadsWithSubjectObjectData;
DROP PROCEDURE GetQuadsWithPredicateObject;
DROP PROCEDURE GetQuadsWithPredicateObjectVirtual;
DROP PROCEDURE GetQuadsWithPredicateObjectData;

-- Drop the Roles

DROP ROLE rdf_admin;
DROP ROLE rdf_readwrite;
DROP ROLE rdf_readinsert;
DROP ROLE rdf_readonly;