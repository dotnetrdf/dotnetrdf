SELECT COUNT(nodeID) As [Unique Nodes] FROM NODES;
SELECT COUNT(tripleID) AS [Unique Triples] FROM TRIPLES;
SELECT COUNT(graphID) AS [Total Graphs] FROM GRAPHS;
SELECT COUNT(graphID) AS [Total Graph Triples] FROM GRAPH_TRIPLES;

--SELECT COUNT(T.tripleID) AS UnusedTriples FROM TRIPLES T LEFT OUTER JOIN GRAPH_TRIPLES G ON T.tripleID=G.tripleID WHERE graphID IS NULL;
-- Unused Nodes only finds Nodes that aren't referenced in the TRIPLES Table
-- This doesn't take account of the fact that a Node may appear in an Unused Triple and thus is unused by definition
-- Unfortunately this would be horrible to express as a SQL Query so it isn't done
--SELECT COUNT(nodeID) AS UnusedNodes FROM ((NODES N LEFT OUTER JOIN TRIPLES T ON nodeID=T.tripleSubject) LEFT OUTER JOIN TRIPLES S ON nodeID=S.triplePredicate) LEFT OUTER JOIN TRIPLES U ON nodeID=U.tripleObject WHERE T.tripleID IS NULL AND S.tripleID IS NULL AND U.tripleID IS NULL;
