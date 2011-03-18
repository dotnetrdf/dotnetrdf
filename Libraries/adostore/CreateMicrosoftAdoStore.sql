-- Create the Graphs Table
CREATE TABLE GRAPHS (graphID INT IDENTITY(1,1) CONSTRAINT GraphPKey PRIMARY KEY,
					 graphUri NVARCHAR(MAX) NULL,
					 graphUriIndex AS CAST(graphUri AS NVARCHAR(450)));
					 
-- Create Indexes on the Graphs Table

CREATE INDEX GraphIndexUri ON GRAPHS (graphUriIndex);
					 
-- Create the Triples Table
CREATE TABLE TRIPLES (subjectID INT NOT NULL,
					  predicateID INT NOT NULL,
					  objectID INT NOT NULL,
					  graphID INT NOT NULL,
					  CONSTRAINT TriplesPKey PRIMARY KEY (subjectID, predicateID, objectID, graphID));
					  
-- Create Indexes on the Triples Table
CREATE INDEX TripleIndexSPO ON TRIPLES (subjectID, predicateID, objectID);

CREATE INDEX TripleIndexS ON TRIPLES (subjectID);

CREATE INDEX TripleIndexSP ON TRIPLES (subjectID, predicateID);

CREATE INDEX TripleIndexSO ON TRIPLES (subjectID, objectID);

CREATE INDEX TripleIndexP ON TRIPLES (predicateID);

CREATE INDEX TripleIndexPO ON TRIPLES (predicateID, objectID);

CREATE INDEX TripleIndexO ON TRIPLES (objectID);

CREATE INDEX TripleIndexG ON TRIPLES (graphID);

-- Create the Nodes Table
CREATE TABLE NODES (nodeID INT IDENTITY(1,1) CONSTRAINT NodePKey PRIMARY KEY,
					nodeType TINYINT NOT NULL,
					nodeValue NVARCHAR(MAX) COLLATE Latin1_General_BIN NOT NULL,
					nodeMeta NVARCHAR(MAX) COLLATE Latin1_General_BIN NULL,
					nodeValueIndex AS CAST(nodeValue AS NVARCHAR(450)));
					
-- Create Indexes on the Nodes Table

CREATE INDEX NodesIndexType ON NODES (nodeType);

CREATE INDEX NodesIndexValue ON NODES (nodeValueIndex);

-- Start Stored Procedures Creation

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
  SET NOCOUNT ON;
  DECLARE @id int;
  SET @id = (SELECT graphID FROM GRAPHS WHERE graphUri=@graphUri);
  RETURN @id;
END
  
-- GetOrCreateGraphID
GO
CREATE PROCEDURE GetOrCreateGraphID @graphUri nvarchar(MAX)
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id int
  EXEC @id = GetGraphID @graphUri;
  IF @id = 0
    BEGIN
      INSERT INTO GRAPHS (graphUri) VALUES (@graphUri);
      EXEC @id = GetGraphID @graphUri;
      RETURN @id;
    END
  ELSE
    RETURN @id;
END

-- DeleteGraph
GO
CREATE PROCEDURE DeleteGraph @graphID int
AS
BEGIN
	SET NOCOUNT ON;
END

-- GetNodeID
GO
CREATE PROCEDURE GetNodeID @nodeType tinyint, @nodeValue nvarchar(MAX), @nodeMeta nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	
	DECLARE @id int;
	IF LEN(@nodeValue) > 450
	  BEGIN
	    -- Get the value for use with the coarse index
	    DECLARE @partialValue nvarchar(450) = SUBSTRING(@nodeValue, 0, 449);
	    SET @partialValue = @partialValue + '%';
	
	    --PRINT 'Using Coarse Value Index Lookup';
	    IF @nodeMeta <> NULL
	      SET @id = (SELECT nodeID FROM NODES
	      WHERE nodeType=@nodeType AND nodeValueIndex LIKE @partialValue AND nodeValue=@nodeValue AND nodeMeta=@nodeMeta);
	    ELSE
	      SET @id = (SELECT nodeID FROM NODES
	      WHERE nodeType=@nodeType AND nodeValueIndex LIKE @partialValue AND nodeValue=@nodeValue);
	  END
	ELSE
	  BEGIN
	    --PRINT 'Using Direct Value Lookup';
	    IF @nodeMeta <> NULL
	      SET @id = (SELECT nodeID FROM NODES WHERE nodeType=@nodeType AND nodeValueIndex=@nodeValue AND nodeMeta=@nodeMeta);
	    ELSE
	      SET @id = (SELECT nodeID FROM NODES WHERE nodeType=@nodeType AND nodeValueIndex=@nodeValue);
	  END
	  
	RETURN @id;
END

-- GetOrCreateNodeID
GO
CREATE PROCEDURE GetOrCreateNodeID @nodeType tinyint, @nodeValue nvarchar(MAX), @nodeMeta nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
	EXEC @id = GetNodeID @nodeType, @nodeValue, @nodeMeta;
	IF @id = 0
	  BEGIN
	    INSERT INTO NODES (nodeType, nodeValue, nodeMeta) VALUES (@nodeType, @nodeValue, @nodeMeta);
	    EXEC @id = GetNodeID @nodeType, @nodeValue, @nodeMeta;
	    RETURN @id;
	  END
	ELSE
	  RETURN @id;
END

-- HasTriple
GO
CREATE PROCEDURE HasTriple @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id int;
  SET @id = (SELECT graphID FROM TRIPLES WHERE subjectID=@subjectID AND predicateID=@predicateID AND objectID=@objectID AND graphID=@graphID);
  IF @id > 0
    RETURN 1;
  ELSE
    RETURN 0;
END

-- AssertTriple
GO
CREATE PROCEDURE AssertTriple @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
	EXEC @id = HasTriple @subjectID, @predicateID, @objectID, @graphID;
	IF @id = 0
	  INSERT INTO TRIPLES (subjectID, predicateID, objectID, graphID) VALUES (@subjectID, @predicateID, @objectID, @graphID);
	  
END

-- AssertTripleFull
GO
CREATE PROCEDURE AssertTripleFull @subjectType tinyint, @subjectValue nvarchar(MAX), @subjectMeta nvarchar(MAX),
								  @predicateType tinyint, @predicateValue nvarchar(MAX), @predicateMeta nvarchar(MAX),
								  @objectType tinyint, @objectValue nvarchar(MAX), @objectMeta nvarchar(MAX),
								  @graphUri nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @s int, @p int, @o int, @g int
	EXEC @s = GetOrCreateNodeID @subjectType, @subjectValue, @subjectMeta;
	EXEC @p = GetOrCreateNodeID @predicateType, @predicateValue, @predicateMeta;
	EXEC @o = GetOrCreateNodeID @objectType, @objectValue, @objectMeta;
	EXEC @g = GetOrCreateGraphID @graphUri;
	
	EXEC AssertTriple @s, @p, @o, @g;
END

-- RetractTriple
GO
CREATE PROCEDURE RetractTriple @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM TRIPLES WHERE subjectID=@subjectID AND predicateID=@predicateID AND objectID=@objectID AND @graphID=graphID;
	RETURN 1;
END

-- RetractTripleFull
GO
CREATE PROCEDURE RetractTripleFull @subjectType tinyint, @subjectValue nvarchar(MAX), @subjectMeta nvarchar(MAX),
								   @predicateType tinyint, @predicateValue nvarchar(MAX), @predicateMeta nvarchar(MAX),
								   @objectType tinyint, @objectValue nvarchar(MAX), @objectMeta nvarchar(MAX),
								   @graphUri nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @s int, @p int, @o int, @g int
	EXEC @s = GetNodeID @subjectType, @subjectValue, @subjectMeta;
	EXEC @p = GetNodeID @predicateType, @predicateValue, @predicateMeta;
	EXEC @o = GetNodeID @objectType, @objectValue, @objectMeta;
	EXEC @g = GetGraphID @graphUri;
	
	EXEC RetractTriple @s, @p, @o, @g;
END

-- End of Stored Procedure Creation
GO

-- Create roles

CREATE ROLE rdf_readwrite;
CREATE ROLE rdf_readinsert;
CREATE ROLE rdf_readonly;

-- Grant Table related permissions for rdf_readonly

GRANT SELECT ON GRAPHS TO rdf_readonly;
GRANT SELECT ON TRIPLES TO rdf_readonly;
GRANT SELECT ON NODES TO rdf_readonly;

-- Grant Table related permissions for rdf_readinsert

GRANT SELECT, INSERT ON GRAPHS TO rdf_readinsert;
GRANT SELECT, INSERT ON TRIPLES TO rdf_readinsert;
GRANT SELECT, INSERT ON NODES TO rdf_readinsert;

-- Grant Table related permissions for rdf_readwrite

GRANT SELECT, INSERT, DELETE ON GRAPHS TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON TRIPLES TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON NODES TO rdf_readwrite;

-- Grant Stored Procedures permissions to roles

GRANT EXECUTE ON GetVersion TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetGraphID TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetOrCreateGraphID TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON GetNodeID TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetOrCreateNodeID TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON HasTriple TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON AssertTriple TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON AssertTripleFull TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON RetractTriple TO rdf_readwrite;
GRANT EXECUTE ON RetractTripleFull TO rdf_readwrite;

-- TEMP - Grant rdf_readonly role to example user for testing

EXEC sp_addrolemember 'rdf_readonly', 'example';