using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Sparql
{
    [TestClass]
    public class ResultSetWriterTests
    {
        [TestMethod]
        public void SparqlXmlWriter()
        {
            try
            {
                Graph g = new Graph();
                FileLoader.Load(g, "InferenceTest.ttl");

                Object results = g.ExecuteQuery("SELECT * WHERE {?s ?p ?o}");
                if (results is SparqlResultSet)
                {
                    TestTools.ShowResults(results);
                }

                StringBuilder output = new StringBuilder();
                System.IO.StringWriter writer = new System.IO.StringWriter(output);
                SparqlXmlWriter sparqlWriter = new SparqlXmlWriter();
                sparqlWriter.Save((SparqlResultSet)results, writer);

                Console.WriteLine();
                Console.WriteLine(output.ToString());
                Console.WriteLine();

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet results2 = new SparqlResultSet();
                StringParser.ParseResultSet(results2, output.ToString());

                Assert.AreEqual(((SparqlResultSet)results).Count, results2.Count, "Result Sets should have contained same number of Results");
                Assert.IsTrue(((SparqlResultSet)results).Equals(results2), "Result Sets should have been equal");
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }
    }
}
