using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;

namespace VDS.RDF.Test.Sparql
{
    /// <summary>
    /// Summary description for SparqlNewFunctions
    /// </summary>
    [TestClass]
    public class SparqlNewFunctions
    {
        [TestMethod]
        public void SparqlIsNumeric()
        {
            Graph g = new Graph();
            UriNode subj = g.CreateUriNode(new Uri("http://example.org/subject"));
            UriNode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));

            g.Assert(subj, pred, (12).ToLiteral(g));
            g.Assert(subj, pred, g.CreateLiteralNode("12"));
            g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonNegativeInteger)));
            g.Assert(subj, pred, g.CreateLiteralNode("12", new Uri(XmlSpecsHelper.XmlSchemaDataTypeNonPositiveInteger)));
            g.Assert(subj, pred, g.CreateLiteralNode("1200", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
            g.Assert(subj, pred, ((byte)50).ToLiteral(g));
            g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeByte)));
            g.Assert(subj, pred, g.CreateLiteralNode("-50", new Uri(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte)));
            g.Assert(subj, pred, g.CreateUriNode(new Uri("http://example.org")));

            TripleStore store = new TripleStore();
            store.Add(g);

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT ?obj (IsNumeric(?obj) AS ?IsNumeric) WHERE { ?s ?p ?obj }");

            Object results = store.ExecuteQuery(q);

            Assert.IsTrue(results is SparqlResultSet, "Result should be a SPARQL Result Set");
            TestTools.ShowResults(results);
        }
    }
}
