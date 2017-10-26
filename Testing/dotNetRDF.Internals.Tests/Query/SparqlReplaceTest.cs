using System.IO;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;

namespace VDS.RDF.Query
{

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

        private const string ReplaceHavingWorkaroundQuery = @"
SELECT (SAMPLE(?o) AS ?sampled) (REPLACE(?sampled, ""1"", ""2"") AS ?oo)
WHERE
{
    ?s ?p ?o
}
GROUP BY ?s
HAVING (COUNT(?p) = 1)
";

        [Fact]
        public void SparqlFunctionsReplace()
        {
            Test(ReplaceQuery);
        }

        [Fact]
        public void SparqlFunctionsHaving()
        {
            Test(HavingQuery, "1");
        }

        [Fact]
        public void SparqlFunctionsReplaceHaving1()
        {
            Test(ReplaceHavingQuery);
        }

        [Fact]
        public void SparqlFunctionsReplaceHaving2()
        {
            Test(ReplaceHavingWorkaroundQuery);
        }

        private static void Test(string query)
        {
            Test(query, "2");
        }

        private static void Test(string query, string literal)

        {
            IGraph graph = new Graph();
            graph.LoadFromString(TestData);

            IInMemoryQueryableStore store = new TripleStore();
            store.Add(graph);
            IQueryableStorage storage = new InMemoryManager(store);

            using (SparqlResultSet resultSet = (SparqlResultSet) storage.Query(query))
            {
                TestTools.ShowResults(resultSet);
                Assert.Equal(1, resultSet.Count);

                SparqlResult result = resultSet[0];
                Assert.True(result.HasBoundValue("oo"));
                Assert.Equal(graph.CreateLiteralNode(literal), result["oo"]);
            }
        }
    }
}