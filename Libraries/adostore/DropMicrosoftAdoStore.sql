-- Drop the Tables

DROP TABLE QUADS;
DROP TABLE GRAPHS;
DROP TABLE NODES;
DROP TABLE BNODES;

-- Drop the Stored Procedures

DROP PROCEDURE GetVersion;

DROP PROCEDURE GetGraphID;
DROP PROCEDURE GetOrCreateGraphID;
DROP PROCEDURE ClearGraph;
DROP PROCEDURE ClearGraphByUri;
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
DROP PROCEDURE GetQuadsData;

DROP PROCEDURE GetGraphQuads;
DROP PROCEDURE GetGraphQuadsData;

DROP PROCEDURE GetQuadsWithSubject;
DROP PROCEDURE GetQuadsWithSubjectData;
DROP PROCEDURE GetQuadsWithPredicate;
DROP PROCEDURE GetQuadsWithPredicateData;
DROP PROCEDURE GetQuadsWithObject;
DROP PROCEDURE GetQuadsWithObjectData;
DROP PROCEDURE GetQuadsWithSubjectPredicate;
DROP PROCEDURE GetQuadsWithSubjectPredicateData;
DROP PROCEDURE GetQuadsWithSubjectObject;
DROP PROCEDURE GetQuadsWithSubjectObjectData;
DROP PROCEDURE GetQuadsWithPredicateObject;
DROP PROCEDURE GetQuadsWithPredicateObjectData;

-- TEMP - Remove example user from rdf_readwrite role

EXEC sp_droprolemember 'rdf_readwrite', 'example';

-- Drop the Roles

DROP ROLE rdf_readwrite;
DROP ROLE rdf_readinsert;
DROP ROLE rdf_readonly;