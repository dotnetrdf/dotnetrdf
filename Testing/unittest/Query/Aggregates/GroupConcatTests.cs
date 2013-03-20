using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query.Datasets;

namespace VDS.RDF.Query.Aggregates
{
    [TestClass]
    public class GroupConcatTests
    {
        private SparqlQueryParser _parser = new SparqlQueryParser();

        private void RunTest(IGraph g, String query, int expected, String var, bool expectNotNull, String expectMatch)
        {
            SparqlQuery q = this._parser.ParseFromString(query);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(new InMemoryDataset(g));

            SparqlResultSet results = processor.ProcessQuery(q) as SparqlResultSet;
            Assert.IsNotNull(results);

            Assert.AreEqual(expected, results.Count);
            Assert.IsTrue(results.Variables.Contains(var));

            foreach (SparqlResult r in results)
            {
                Assert.IsTrue(r.HasValue(var));
                if (expectNotNull)
                {
                    Assert.IsTrue(r.HasBoundValue(var));
                    INode value = r[var];
                    Assert.AreEqual(NodeType.Literal, value.NodeType);
                    String lexValue = ((ILiteralNode)value).Value;
                    Assert.IsTrue(lexValue.Contains(expectMatch));
                }
                else
                {
                    Assert.IsFalse(r.HasBoundValue(var));
                }
            }
        }

        [TestMethod]
        public void SparqlGroupConcat1()
        {
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/ns#"));
            g.Assert(g.CreateUriNode("ex:subject"), g.CreateUriNode("ex:predicate"), g.CreateLiteralNode("object"));

            this.RunTest(g, "SELECT (GROUP_CONCAT(?o) AS ?concat) WHERE { ?s ?p ?o }", 1, "concat", true, "object");
        }

        [TestMethod]
        public void SparqlGroupConcat2()
        {
            IGraph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/ns#"));
            g.Assert(g.CreateUriNode("ex:subject"), g.CreateUriNode("ex:predicate"), g.CreateLiteralNode("object"));

            this.RunTest(g, "SELECT (GROUP_CONCAT(?s) AS ?concat) WHERE { ?s ?p ?o }", 1, "concat", true, "subject");
        }

        [TestMethod]
        public void SparqlGroupConcat3()
        {
            IGraph g = new Graph();

            String query = @"SELECT (GROUP_CONCAT(?x) AS ?concat)
WHERE
{
  VALUES ( ?x )
  {
    ( 'string' )
    ( 1234 )
    ( true )
  }
}";
            this.RunTest(g, query, 1, "concat", true, "1234");
        }
    }
}
