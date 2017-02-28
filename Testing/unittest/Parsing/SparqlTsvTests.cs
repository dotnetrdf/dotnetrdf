using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query;

namespace VDS.RDF.Parsing
{
    [TestFixture]
    public class SparqlTsvTests
    {
        private readonly SparqlTsvParser _parser = new SparqlTsvParser();

        private void CheckVariables(SparqlResultSet results, params String[] vars)
        {
            foreach (String var in vars)
            {
                Assert.IsTrue(results.Variables.Contains(var), "Missing variable ?" + var);
            }
        }

        [Test]
        public void ParsingSparqlTsv01()
        {
            const String data = "?x\t?y\n"
                                + "<http://x>\t<http://y>\n";

            SparqlResultSet results = new SparqlResultSet();
            this._parser.Load(results, new StringReader(data));

            TestTools.ShowResults(results);

            Assert.AreEqual(SparqlResultsType.VariableBindings, results.ResultsType);
            Assert.AreEqual(2, results.Variables.Count());
            CheckVariables(results, "x", "y");
            Assert.AreEqual(1, results.Results.Count);
        }

        [Test]
        public void ParsingSparqlTsv02()
        {
            // Relative URI - CORE-432
            const String data = "?x\t?y\n"
                                + "<http://x>\t<y>\n";

            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }

        [Test]
        public void ParsingSparqlTsv03()
        {
            // Invalid URI - CORE-432
            const String data = "?x\t?y\n"
                                + "<http://x a bad uri>\t<y>\n";

            SparqlResultSet results = new SparqlResultSet();

            Assert.Throws<RdfParseException>(() => this._parser.Load(results, new StringReader(data)));
        }
    }
}