--SELECT nodeID FROM NODES WHERE
--nodeType=1 AND nodeValueIndex='http://example.org/graph';

DECLARE @g int;
EXEC @g = GetOrCreateGraphID N'http://example.org/graph';
PRINT 'Created Graph ID: ' + STR(@g);

EXEC @g = GetOrCreateGraphID N'http://example.org/graph';
PRINT 'Retrieved Graph ID: ' + STR(@g);

DECLARE @s int, @p int, @o1 int, @o2 int;
EXEC @s = GetOrCreateNodeID 1, N'http://example.org/subject';
PRINT 'Created Subject ID: ' + STR(@s);

EXEC @p = GetOrCreateNodeID 1, N'http://example.org/predicate';
PRINT 'Created Predicate ID: ' + STR(@p);

EXEC @o1 = GetOrCreateNodeID 2, N'Hello', '@en';
PRINT 'Created Object ID: ' + Str(@o1);

EXEC @o2 = GetOrCreateNodeID 2, N'Bonjour', '@fr';
PRINT 'Created Object ID: ' + Str(@o2);

EXEC AssertTriple @s, @p, @o1, @g;
DECLARE @exists int;
EXEC @exists = HasTriple @s, @p, @o1, @g;
IF @exists = 1
  PRINT 'Triple Exists';
ELSE
  PRINT 'Triple does not exist';

EXEC @exists = HasTriple @s, @p, @o2, @g;
IF @exists = 1
  PRINT 'Triple Exists';
ELSE
  PRINT 'Triple does not exist';
  
EXEC AssertTripleFull 1, N'ex:subject', NULL, 1, N'ex:predicate', NULL, 0, N'_:autos1', NULL, 1;