using System.IO;
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Query
{
    [TestFixture]
    public class SparqlReplaceTest
    {
        private const string TestData = @"<http://r> <http://r> ""1"" .";

        private const string ReplaceQuery = @"
SELECT (REPLACE(SAMPLE(?o), ""1"", ""2"") AS ?oo)
WHERE
{
    ?s ?p ?o
}
GROUP BY ?s
";

        private const string HavingQuery = @"
SELECT (SAMPLE(?o) AS ?oo)
WHERE
{
    ?s ?p ?o
}
GROUP BY ?s
HAVING (COUNT(?p) = 1)
";

        private const string ReplaceHavingQuery = @"
SELECT (REPLACE(SAMPLE(?o), ""1"", ""2"") AS ?oo)
WHERE
{
    ?s ?p ?o
}
GROUP BY ?s
HAVING (COUNT(?p) = 1)
";

        [Test]
        public void SparqlFunctionsReplaceTest()
        {
            Test(ReplaceQuery);
        }

        [Test]
        public void SparqlFunctionsHavingTest()
        {
            Test(HavingQuery);
        }

        [Test]
        public void SparqlFunctionsReplaceHavingTest()
        {
            Test(ReplaceHavingQuery);
        }

        private static void Test(string query)
        {
            IGraph graph = new Graph();
            graph.LoadFromString(TestData);

            IInMemoryQueryableStore store = new TripleStore();
            store.Add(graph);
            IQueryableStorage storage = new InMemoryManager(store);

            using (SparqlResultSet resultSet = (SparqlResultSet)storage.Query(query))
            {
                Assert.AreEqual(1, resultSet.Count);
            }
        }
    }
}
