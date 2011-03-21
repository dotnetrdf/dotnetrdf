--SELECT nodeID FROM NODES WHERE
--nodeType=1 AND nodeValueIndex='http://example.org/graph';

-- Graph ID Creation Tests

PRINT 'Graph Creation Tests'

DECLARE @g int;
EXEC @g = GetOrCreateGraphID N'http://example.org/graph';
PRINT 'Created Graph ID: ' + STR(@g);

EXEC @g = GetOrCreateGraphID N'http://example.org/graph';
PRINT 'Retrieved Graph ID: ' + STR(@g);

EXEC @g = GetOrCreateGraphID NULL
PRINT 'Created Graph ID: ' + STR(@g);

EXEC @g = GetOrCreateGraphID NULL
PRINT 'Retrieved Graph ID: ' + STR(@g);

PRINT '';

-- Node ID Creation Tests

PRINT 'Node Creation Tests';

DECLARE @s int, @p int, @o1 int, @o2 int;
EXEC @s = GetOrCreateNodeID 1, N'http://example.org/subject';
PRINT 'Created Subject ID: ' + STR(@s);

EXEC @p = GetOrCreateNodeID 1, N'http://example.org/predicate';
PRINT 'Created Predicate ID: ' + STR(@p);

EXEC @o1 = GetOrCreateNodeID 2, N'Hello', '@en';
PRINT 'Created Object ID: ' + Str(@o1);

EXEC @o2 = GetOrCreateNodeID 2, N'Bonjour', '@fr';
PRINT 'Created Object ID: ' + Str(@o2);

PRINT '';

-- Assert Quads Tests

PRINT 'Triple Assertion Tests';

EXEC AssertQuad @s, @p, @o1, @g;
DECLARE @exists int;
EXEC @exists = HasQuad @s, @p, @o1, @g;
IF @exists = 1
  PRINT 'Triple Exists (Pass)';
ELSE
  PRINT 'Triple does not exist (Fail)';

EXEC @exists = HasQuad @s, @p, @o2, @g;
IF @exists = 1
  PRINT 'Triple Exists (Fail)';
ELSE
  PRINT 'Triple does not exist (Pass)';
  
EXEC AssertQuadData 1, N'ex:subject', NULL, 1, N'ex:predicate', NULL, 0, N'_:autos1', NULL, @g;

PRINT '';

---- Retract Quad Tests

--PRINT 'Triple Retraction Tests'

--Exec RetractQuad @s, @p, @o1, @g;
--EXEC @exists = HasQuad @s, @p, @o1, @g;
--IF @exists = 1
--  PRINT 'Triple Exists (Fail)';
--ELSE
--  PRINT 'Triple does not exist (Pass)';
  
--EXEC RetractQuadData 1, N'ex:subject', NULL, 1, N'ex:predicate', NULL, 0, N'_:autos1', NULL, 1;
--EXEC @exists = HasQuadData 1, N'ex:subject', NULL, 1, N'ex:predicate', NULL, 0, N'_:autos1', NULL, 1;
--IF @exists = 1
--  PRINT 'Triple Exists (Fail)';
--ELSE
--  PRINT 'Triple does not exist (Pass)';
  
--PRINT '';

---- Delete Graph Tests

--PRINT 'Graph Deletion Tests';

--EXEC @g = DeleteGraph @g;
--IF @g = 1
--  PRINT 'Graph Deleted (Pass)'
--ELSE
--  PRINT 'Graph not deleted (Fail)'
  
--EXEC @g = DeleteGraphByUri N'http://example.org/noSuchGraph'
--IF @g = 1
--  PRINT 'Graph Deleted (Fail)'
--ELSE
--  PRINT 'Graph not deleted (Pass)'
  
--PRINT '';