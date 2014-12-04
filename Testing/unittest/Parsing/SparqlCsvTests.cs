using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    [TestFixture]
    public class SparqlCsvTests
    {
        private readonly SparqlCsvParser _parser = new SparqlCsvParser();

        private void CheckVariables(SparqlResultSet results, params String[] vars)
        {
            foreach (String var in vars)
            {
                Assert.IsTrue(results.Variables.Contains(var), "Missing variable ?" + var);
            }
        }

        [Test]
        public void ParsingSparqlCsv01()
        {
            const String data = @"x,y
http://x,http://y
";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            TestTools.ShowResults(results);

            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.AreEqual(2, results.Variables.Count());
            CheckVariables(results, "x", "y");
            Assert.AreEqual(1, results.Results.Count);
        }

        [Test]
        public void ParsingSparqlCsv02()
        {
            // Header row has quoting - CORE-433
            const String data = @"""x"",""y""
http://x,http://y
";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            TestTools.ShowResults(results);

            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.AreEqual(2, results.Variables.Count());
            CheckVariables(results, "x", "y");
            Assert.AreEqual(1, results.Results.Count);
        }

        [Test]
        public void ParsingSparqlCsv03()
        {
            // Invalid URI - CORE-432
            // As CSV is lossy and doesn't distinguish between URIs and literals we treat it as a literal
            const String data = @"""x"",""y""
http://x a bad uri,http://y
";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.AreEqual(2, results.Variables.Count());
            CheckVariables(results, "x", "y");
            Assert.AreEqual(1, results.Results.Count);

            INode n = results.Results[0]["x"];
            Assert.IsNotNull(n);
            Assert.AreEqual(NodeType.Literal, n.NodeType);
        }
    }
}
