--Upgrades a Version 0.1.0 store to a Version 0.1.1 store

--Need to add the Hash field to the GRAPHS table
ALTER TABLE GRAPHS ADD graphHash INT;

--Need to add the Hash field to the NODES table
ALTER TABLE NODES ADD nodeHash INT;
--Remove the existing Primary Key Constraint, change nodeID to none auto-incremented and then recreate constraint
ALTER TABLE NODES DROP CONSTRAINT NodePKey;
ALTER TABLE NODES ALTER COLUMN nodeID INT;
ALTER TABLE NODES ADD CONSTRAINT NodePKey PRIMARY KEY (nodeID);

--Need to add the Hash field to the TRIPLES table
ALTER TABLE TRIPLES ADD tripleHash INT;
--Remove the existing Primary Key Constraint, change tripleID to none auto-increment and then recreate constraint
ALTER TABLE TRIPLES DROP CONSTRAINT TriplePKey;
ALTER TABLE TRIPLES ALTER COLUMN tripleID INT;
ALTER TABLE TRIPLES ADD CONSTRAINT TriplePKey PRIMARY KEY (tripleID);

--Add the Hash field to the NS_URIS table
ALTER TABLE NS_URIS ADD nsUriHash INT;