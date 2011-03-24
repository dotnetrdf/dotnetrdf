-- Create the Graphs Table
CREATE TABLE GRAPHS (graphID INT IDENTITY(1,1) CONSTRAINT GraphPKey PRIMARY KEY,
					 graphUri NVARCHAR(MAX) NULL,
					 graphUriIndex AS CAST(graphUri AS NVARCHAR(450)));
					 
-- Create Indexes on the Graphs Table
CREATE INDEX GraphIndexUri ON GRAPHS (graphUriIndex);
					 
-- Create the Quads Table
CREATE TABLE QUADS (subjectID INT NOT NULL,
					predicateID INT NOT NULL,
					objectID INT NOT NULL,
					graphID INT NOT NULL,
					CONSTRAINT QuadsPKey PRIMARY KEY (subjectID, predicateID, objectID, graphID));
					  
-- Create Indexes on the Quads Table
CREATE INDEX QuadIndexSPO ON QUADS (subjectID, predicateID, objectID);

CREATE INDEX QuadIndexS ON QUADS (subjectID);

CREATE INDEX QuadIndexSP ON QUADS (subjectID, predicateID);

CREATE INDEX QuadIndexSO ON QUADS (subjectID, objectID);

CREATE INDEX QuadIndexP ON QUADS (predicateID);

CREATE INDEX QuadIndexPO ON QUADS (predicateID, objectID);

CREATE INDEX QuadIndexO ON QUADS (objectID);

CREATE INDEX QuadIndexG ON QUADS (graphID);

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
CREATE PROCEDURE GetGraphID @graphUri nvarchar(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id int;
    
  IF @graphUri IS NULL
    SET @id = (SELECT graphID FROM GRAPHS WHERE graphUriIndex IS NULL AND graphUri IS NULL);
  ELSE
    IF LEN(@graphUri) > 450
      BEGIN
		-- Get the value for use with the coarse index
	    DECLARE @partialValue nvarchar(450) = SUBSTRING(@graphUri, 0, 449);
	    SET @partialValue = @partialValue + '%';
	    
		SET @id = (SELECT graphID FROM GRAPHS WHERE graphUriIndex LIKE @partialValue AND graphUri=@graphUri);
	  END
	ELSE
	  SET @id = (SELECT graphID FROM GRAPHS WHERE graphUriIndex=@graphUri AND graphUri=@graphUri);
  RETURN @id;
END
  
-- GetOrCreateGraphID
GO
CREATE PROCEDURE GetOrCreateGraphID @graphUri nvarchar(MAX) = NULL
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id int
  EXEC @id = GetGraphID @graphUri;
  IF @id = 0
    BEGIN
      INSERT INTO GRAPHS (graphUri) VALUES (@graphUri);
      RETURN SCOPE_IDENTITY();
    END
  ELSE
    RETURN @id;
END

-- ClearGraph
GO
CREATE PROCEDURE ClearGraph @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	IF @graphID > 0
	  BEGIN
		DELETE FROM QUADS WHERE graphID=@graphID;
		RETURN 1;
	  END
	ELSE
	  RETURN 0;
END

-- ClearGraphByUri
GO
CREATE PROCEDURE ClearGraphByUri @graphUri nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
	EXEC @id = GetGraphID @graphUri;
	IF @id > 0
	  BEGIN
	    EXEC @id = ClearGraph @id;
	    RETURN @id;
	  END
	ELSE
	  RETURN 0;
END

-- DeleteGraph
GO
CREATE PROCEDURE DeleteGraph @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	IF @graphID > 0
	  BEGIN
		DELETE FROM GRAPHS WHERE graphID=@graphID;
		DELETE FROM QUADS WHERE graphID=@graphID;
		RETURN 1;
	  END
	ELSE
	  RETURN 0;
END

-- DeleteGraphByUri
GO
CREATE PROCEDURE DeleteGraphByUri @graphUri nvarchar(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
	EXEC @id = GetGraphID @graphUri;
	IF @id > 0
	  BEGIN
	    EXEC @id = DeleteGraph @id;
	    RETURN @id;
	  END
	ELSE
	  RETURN 0;
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
	    IF @nodeMeta IS NULL
          SET @id = (SELECT nodeID FROM NODES
	      WHERE nodeType=@nodeType AND nodeValueIndex LIKE @partialValue AND nodeValue=@nodeValue AND nodeMeta IS NULL);
	    ELSE
	      SET @id = (SELECT nodeID FROM NODES
	      WHERE nodeType=@nodeType AND nodeValueIndex LIKE @partialValue AND nodeValue=@nodeValue AND nodeMeta=@nodeMeta);  
	  END
	ELSE
	  BEGIN
	    --PRINT 'Using Direct Value Lookup';
	    IF @nodeMeta IS NULL
	      SET @id = (SELECT nodeID FROM NODES WHERE nodeType=@nodeType AND nodeValueIndex=@nodeValue AND nodeMeta IS NULL);
	    ELSE
	      SET @id = (SELECT nodeID FROM NODES WHERE nodeType=@nodeType AND nodeValueIndex=@nodeValue AND nodeMeta=@nodeMeta);
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
	    RETURN SCOPE_IDENTITY();
	  END
	ELSE
	  RETURN @id;
END

-- GetNodeData
GO
CREATE PROCEDURE GetNodeData @nodeID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT nodeType, nodeValue, nodeMeta FROM NODES WHERE nodeID=@nodeID;
END

-- HasQuad
GO
CREATE PROCEDURE HasQuad @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
  SET NOCOUNT ON;
  DECLARE @id int;
  SET @id = (SELECT graphID FROM QUADS WHERE subjectID=@subjectID AND predicateID=@predicateID AND objectID=@objectID AND graphID=@graphID);
  IF @id > 0
    RETURN 1;
  ELSE
    RETURN 0;
END

-- HasQuadData
GO
CREATE PROCEDURE HasQuadData @subjectType tinyint, @subjectValue nvarchar(MAX), @subjectMeta nvarchar(MAX) = NULL,
						     @predicateType tinyint, @predicateValue nvarchar(MAX), @predicateMeta nvarchar(MAX) = NULL,
							 @objectType tinyint, @objectValue nvarchar(MAX), @objectMeta nvarchar(MAX) = NULL,
						     @graphUri nvarchar(MAX)
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @s int, @p int, @o int, @g int
	EXEC @s = GetNodeID @subjectType, @subjectValue, @subjectMeta;
	IF @s = 0 RETURN 0;
	EXEC @p = GetNodeID @predicateType, @predicateValue, @predicateMeta;
	IF @p = 0 RETURN 0;
	EXEC @o = GetNodeID @objectType, @objectValue, @objectMeta;
	IF @o = 0 RETURN 0;
	EXEC @g = GetGraphID @graphUri;
	IF @g = 0 RETURN 0;
	
	DECLARE @id int;
	EXEC @id = HasQuad @s, @p, @o, @g;
	IF @id > 0
	  RETURN 1;
	ELSE
	  RETURN 0;
END

-- AssertQuad
GO
CREATE PROCEDURE AssertQuad @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @id int;
	EXEC @id = HasQuad @subjectID, @predicateID, @objectID, @graphID;
	IF @id = 0
	  INSERT INTO QUADS (subjectID, predicateID, objectID, graphID) VALUES (@subjectID, @predicateID, @objectID, @graphID);
	  
END

-- AssertQuadData
GO
CREATE PROCEDURE AssertQuadData @subjectType tinyint, @subjectValue nvarchar(MAX), @subjectMeta nvarchar(MAX) = NULL,
								@predicateType tinyint, @predicateValue nvarchar(MAX), @predicateMeta nvarchar(MAX) = NULL,
								@objectType tinyint, @objectValue nvarchar(MAX), @objectMeta nvarchar(MAX) = NULL,
								@graphID int
AS
BEGIN
	SET NOCOUNT ON;
	DECLARE @s int, @p int, @o int
	EXEC @s = GetOrCreateNodeID @subjectType, @subjectValue, @subjectMeta;
	EXEC @p = GetOrCreateNodeID @predicateType, @predicateValue, @predicateMeta;
	EXEC @o = GetOrCreateNodeID @objectType, @objectValue, @objectMeta;
	
	EXEC AssertQuad @s, @p, @o, @graphID;
END

-- RetractQuad
GO
CREATE PROCEDURE RetractQuad @subjectID int, @predicateID int, @objectID int, @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	DELETE FROM QUADS WHERE subjectID=@subjectID AND predicateID=@predicateID AND objectID=@objectID AND @graphID=graphID;
	RETURN 1;
END

-- RetractQuadData
GO
CREATE PROCEDURE RetractQuadData @subjectType tinyint, @subjectValue nvarchar(MAX), @subjectMeta nvarchar(MAX) = NULL,
								 @predicateType tinyint, @predicateValue nvarchar(MAX), @predicateMeta nvarchar(MAX) = NULL,
								 @objectType tinyint, @objectValue nvarchar(MAX), @objectMeta nvarchar(MAX) = NULL,
								 @graphID int
AS
BEGIN
	SET NOCOUNT ON;
	
	IF @graphID = 0 RETURN 0;
	
	DECLARE @s int, @p int, @o int
	EXEC @s = GetNodeID @subjectType, @subjectValue, @subjectMeta;
	IF @s = 0 RETURN 0;
	EXEC @p = GetNodeID @predicateType, @predicateValue, @predicateMeta;
	IF @p = 0 RETURN 0;
	EXEC @o = GetNodeID @objectType, @objectValue, @objectMeta;
	IF @o = 0 RETURN 0;
	
	EXEC RetractQuad @s, @p, @o, @graphID;
	RETURN 1;
END

-- GetGraphQuads
GO
CREATE PROCEDURE GetGraphQuads @graphID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT subjectID, predicateID, objectID FROM QUADS WHERE graphID=@graphID;
END

-- GetGraphQuadsData
GO
CREATE PROCEDURE GetGraphQuadsData @graphID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT S.nodeType AS subjectType, S.nodeValue AS subjectValue, S.nodeMeta AS subjectMeta,
         P.nodeType AS predicateType, P.nodeValue AS predicateValue, P.nodeMeta AS predicateMeta,
         O.nodeType AS objectType, O.nodeValue AS objectValue, O.nodeMeta AS objectMeta
  FROM QUADS Q
  INNER JOIN NODES S ON Q.subjectID=S.nodeID
  INNER JOIN NODES P ON Q.predicateID=P.nodeID
  INNER JOIN NODES O ON Q.objectID=O.nodeID
  WHERE graphID=@graphID;  
END

-- GetQuads
GO
CREATE PROCEDURE GetQuads
AS
BEGIN
  SET NOCOUNT ON;
  SELECT subjectID, predicateID, objectID, graphID FROM QUADS;
END

-- GetQuadsData
GO
CREATE PROCEDURE GetQuadsData
AS
BEGIN
  SET NOCOUNT ON;
  SELECT S.nodeType AS subjectType, S.nodeValue AS subjectValue, S.nodeMeta AS subjectMeta,
         P.nodeType AS predicateType, P.nodeValue AS predicateValue, P.nodeMeta AS predicateMeta,
         O.nodeType AS objectType, O.nodeValue AS objectValue, O.nodeMeta AS objectMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES S ON Q.subjectID=S.nodeID
  INNER JOIN NODES P ON Q.predicateID=P.nodeID
  INNER JOIN NODES O ON Q.objectID=O.nodeID;    
END

-- GetQuadsWithSubject
GO
CREATE PROCEDURE GetQuadsWithSubject @subjectID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT predicateID, objectID, graphID FROM QUADS WHERE subjectID=@subjectID;
END

-- GetQuadsWithSubjectData
GO
CREATE PROCEDURE GetQuadsWithSubjectData @subjectID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT P.nodeType AS predicateType, P.nodeValue AS predicateValue, P.nodeMeta AS predicateMeta,
         O.nodeType AS objectType, O.nodeValue AS objectValue, O.nodeMeta AS objectMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES P ON Q.predicateID=P.nodeID
  INNER JOIN NODES O ON Q.objectID=O.nodeID
  WHERE subjectID=@subjectID;
END

-- GetQuadsWithPredicate
GO
CREATE PROCEDURE GetQuadsWithPredicate @predicateID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT subjectID, objectID, graphID FROM QUADS WHERE predicateID=@predicateID;
END

-- GetQuadsWithPredicateData
GO
CREATE PROCEDURE GetQuadsWithPredicateData @predicateID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT S.nodeType AS subjectType, S.nodeValue AS subjectValue, S.nodeMeta AS subjectMeta,
         O.nodeType AS objectType, O.nodeValue AS objectValue, O.nodeMeta AS objectMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES S ON Q.subjectID=S.nodeID
  INNER JOIN NODES O ON Q.objectID=O.nodeID
  WHERE predicateID=@predicateID;
END

-- GetQuadsWithObject
GO
CREATE PROCEDURE GetQuadsWithObject @objectID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT subjectID, predicateID, graphID FROM QUADS WHERE objectID=@objectID;
END

-- GetQuadsWithObjectData
GO
CREATE PROCEDURE GetQuadsWithObjectData @objectID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT S.nodeType AS subjectType, S.nodeValue AS subjectValue, S.nodeMeta AS subjectMeta,
         P.nodeType AS predicateType, P.nodeValue AS predicateValue, P.nodeMeta AS predicateMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES S ON Q.subjectID=S.nodeID
  INNER JOIN NODES P ON Q.predicateID=P.nodeID
  WHERE objectID=@objectID;
END  

-- GetQuadsWithSubjectPredicate
GO
CREATE PROCEDURE GetQuadsWithSubjectPredicate @subjectID int, @predicateID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT objectID, graphID FROM QUADS WHERE subjectID=@subjectID AND predicateID=@predicateID;
END

-- GetQuadsWithSubjectPredicateData
GO
CREATE PROCEDURE GetQuadsWithSubjectPredicateData @subjectID int, @predicateID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT nodeType AS objectType, nodeValue AS objectValue, nodeMeta AS objectMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES O ON Q.objectID=O.nodeID
  WHERE subjectID=@subjectID AND predicateID=@predicateID;  
END  

-- GetQuadsWithSubjectObject
GO
CREATE PROCEDURE GetQuadsWithSubjectObject @subjectID int, @objectID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT predicateID, graphID FROM QUADS WHERE subjectID=@subjectID AND objectID=@objectID;
END

-- GetQuadsWithSubjectObjectData
GO
CREATE PROCEDURE GetQuadsWithSubjectObjectData @subjectID int, @objectID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT nodeType AS predicateType, nodeValue AS predicateValue, nodeMeta AS predicateMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES P ON Q.predicateID=P.nodeID
  WHERE subjectID=@subjectID AND objectID=@objectID;
END

-- GetQuadsWithPredicateObject
GO
CREATE PROCEDURE GetQuadsWithPredicateObject @predicateID int, @objectID int
AS
BEGIN
	SET NOCOUNT ON;
	SELECT subjectID, graphID FROM QUADS WHERE predicateID=@predicateID AND objectID=@objectID;
END

-- GetQuadsWithPredicateObjectData
GO
CREATE PROCEDURE GetQuadsWithPredicateObjectData @predicateID int, @objectID int
AS
BEGIN
  SET NOCOUNT ON;
  SELECT nodeType AS subjectType, nodeValue AS subjectValue, nodeMeta AS subjectMeta,
         graphID
  FROM QUADS Q
  INNER JOIN NODES O ON Q.objectID=O.nodeID
  WHERE predicateID=@predicateID AND objectID=@objectID;    
END

-- End of Stored Procedure Creation
GO

-- Create roles

CREATE ROLE rdf_readwrite;
CREATE ROLE rdf_readinsert;
CREATE ROLE rdf_readonly;

-- Grant Table and View related permissions for rdf_readonly

GRANT SELECT ON GRAPHS TO rdf_readonly;
GRANT SELECT ON QUADS TO rdf_readonly;
GRANT SELECT ON NODES TO rdf_readonly;

-- Grant Table and View related permissions for rdf_readinsert
-- Node that it still needs DELETE permission of the QUADS table since
-- inserting a Graph with the same URI as a previous graph requires deleting
-- the existing Quads from that Graph

GRANT SELECT, INSERT ON GRAPHS TO rdf_readinsert;
GRANT SELECT, INSERT, DELETE ON QUADS TO rdf_readinsert;
GRANT SELECT, INSERT ON NODES TO rdf_readinsert;

-- Grant Table and View related permissions for rdf_readwrite

GRANT SELECT, INSERT, DELETE ON GRAPHS TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON QUADS TO rdf_readwrite;
GRANT SELECT, INSERT, DELETE ON NODES TO rdf_readwrite;

-- Grant Stored Procedures permissions to roles

GRANT EXECUTE ON GetVersion TO rdf_readwrite, rdf_readinsert, rdf_readonly;

GRANT EXECUTE ON GetGraphID TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetOrCreateGraphID TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON ClearGraph TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON ClearGraph TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON DeleteGraph TO rdf_readwrite;
GRANT EXECUTE ON DeleteGraphByUri TO rdf_readwrite;

GRANT EXECUTE ON GetNodeID TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetOrCreateNodeID TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON GetNodeData TO rdf_readwrite, rdf_readinsert, rdf_readonly;

GRANT EXECUTE ON HasQuad TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON HasQuadData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON AssertQuad TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON AssertQuadData TO rdf_readwrite, rdf_readinsert;
GRANT EXECUTE ON RetractQuad TO rdf_readwrite;
GRANT EXECUTE ON RetractQuadData TO rdf_readwrite;

GRANT EXECUTE ON GetQuads TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsData TO rdf_readwrite, rdf_readinsert, rdf_readonly;

GRANT EXECUTE ON GetGraphQuads TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetGraphQuadsData TO rdf_readwrite, rdf_readinsert, rdf_readonly;

GRANT EXECUTE ON GetQuadsWithSubject TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithSubjectData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithPredicate TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithPredicateData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithObject TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithObjectData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithSubjectPredicate TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithSubjectPredicateData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithSubjectObject TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithSubjectObjectData TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithPredicateObject TO rdf_readwrite, rdf_readinsert, rdf_readonly;
GRANT EXECUTE ON GetQuadsWithPredicateObjectData TO rdf_readwrite, rdf_readinsert, rdf_readonly;

-- TEMP - Grant rdf_readwrite role to example user for testing

EXEC sp_addrolemember 'rdf_readwrite', 'example';