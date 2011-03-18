DECLARE @ret int;
EXEC @ret = GetOrCreateGraphID N'http://example.org/graph';
PRINT @ret;