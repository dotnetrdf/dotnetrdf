-- Drop the Tables

DROP TABLE TRIPLES;
DROP TABLE GRAPHS;
DROP TABLE NODES;

-- Drop the Stored Procedures

DROP PROCEDURE GetVersion;
DROP PROCEDURE GetGraphID;
DROP PROCEDURE GetOrCreateGraphID;

-- TEMP - Remove example user from rdf_readonly role

EXEC sp_droprolemember 'rdf_readonly', 'example';

-- Drop the Roles

DROP ROLE rdf_readwrite;
DROP ROLE rdf_readonly;