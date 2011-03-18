-- Create the Graphs Table
CREATE TABLE GRAPHS (graphID INT IDENTITY(1,1) CONSTRAINT GraphPKey PRIMARY KEY,
					 graphUri NVARCHAR(MAX) NULL);
					 
-- Create the Triples Table
CREATE TABLE TRIPLES (graphID INT NOT NULL,
					  subjectID INT NOT NULL,
					  predicateID INT NOT NULL,
					  objectID INT NOT NULL,
					  CONSTRAINT TriplesPKey PRIMARY KEY (graphID, subjectID, predicateID, objectID));
					  
-- Create Indexes on the Triples Table
CREATE INDEX TripleIndexSPO ON TRIPLES (subjectID, predicateID, objectID);

CREATE INDEX TripleIndexS ON TRIPLES (subjectID);

CREATE INDEX TripleIndexSP ON TRIPLES (subjectID, predicateID);

CREATE INDEX TripleIndexSO ON TRIPLES (subjectID, objectID);

CREATE INDEX TripleIndexP ON TRIPLES (predicateID);

CREATE INDEX TripleIndexPO ON TRIPLES (predicateID, objectID);

CREATE INDEX TripleIndexO ON TRIPLES (objectID);

-- Create the Nodes Table
CREATE TABLE NODES (nodeID INT IDENTITY(1,1) CONSTRAINT NodePKey PRIMARY KEY,
					nodeType TINYINT NOT NULL,
					nodeValue NVARCHAR(MAX) COLLATE Latin1_General_BIN NOT NULL,
					nodeMeta NVARCHAR(MAX) COLLATE Latin1_General_BIN NOT NULL,
					nodeValueIndex AS CAST(nodeValue AS NVARCHAR(450)));
					
-- Create Indexes on the Nodes Table

CREATE INDEX NodesIndexType ON NODES (nodeType);

CREATE INDEX NodesIndexValue ON NODES (nodeValueIndex);

-- Create Stored Procedures

-- GetVersion
GO
CREATE PROCEDURE GetVersion
AS
  BEGIN
    RETURN 1;
  END
  
-- GetGraphID
GO
CREATE PROCEDURE GetGraphID @graphUri nvarchar(MAX)
AS
BEGIN
  SELECT graphID FROM GRAPHS
  WHERE graphUri=@graphUri;
END
  
-- GetOrCreateGraphID
GO
CREATE PROCEDURE GetOrCreateGraphID @graphUri nvarchar(MAX)
AS
BEGIN
  DECLARE @id Int
  EXEC @id = GetGraphID @graphUri;
  IF @id = NULL
  
  ELSE
    RETURN @id;
  END 
END

GO

-- Create two roles

CREATE ROLE rdf_readwrite;
CREATE ROLE rdf_readonly;

-- Grant Table related permissions for rdf_readonly

GRANT SELECT ON GRAPHS TO rdf_readonly;
GRANT SELECT ON TRIPLES TO rdf_readonly;
GRANT SELECT ON NODES TO rdf_readonly;

-- Grant Table related permissions for rdf_readwrite

GRANT SELECT, INSERT, DELETE ON GRAPHS TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON TRIPLES TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON NODES TO rdf_readwrite;

-- Grant Stored Procedures permissions to roles

GRANT EXECUTE ON GetVersion TO rdf_readwrite, rdf_readonly;
GRANT EXECUTE ON GetGraphID TO rdf_readwrite, rdf_readonly;
GRANT EXECUTE ON GetOrCreateGraphID TO rdf_readwrite;

-- TEMP - Grant rdf_readonly role to example user for testing

EXEC sp_addrolemember 'rdf_readonly', 'example';