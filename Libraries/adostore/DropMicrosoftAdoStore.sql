-- Drop the Tables

DROP TABLE QUADS;
DROP TABLE GRAPHS;
DROP TABLE NODES;

-- Drop the Views

DROP VIEW QUAD_DATA;

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
DROP PROCEDURE HasQuad;
DROP PROCEDURE HasQuadData;
DROP PROCEDURE AssertQuad;
DROP PROCEDURE AssertQuadData;
DROP PROCEDURE RetractQuad;
DROP PROCEDURE RetractQuadData;
DROP PROCEDURE GetQuads;
DROP PROCEDURE GetQuadsData;

-- TEMP - Remove example user from rdf_readwrite role

EXEC sp_droprolemember 'rdf_readwrite', 'example';

-- Drop the Roles

DROP ROLE rdf_readwrite;
DROP ROLE rdf_readinsert;
DROP ROLE rdf_readonly;