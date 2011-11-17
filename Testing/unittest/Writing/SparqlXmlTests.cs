using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SparqlXmlTests
    {
        [TestMethod]
        public void WritingSparqlXmlWithNulls()
        {
            TripleStore store = new TripleStore();
            store.Add(new Graph());
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            store.Add(g);

            Object results = store.ExecuteQuery("SELECT DISTINCT ?g WHERE { GRAPH ?g { ?s ?p ?o } }");
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                SparqlXmlWriter writer = new SparqlXmlWriter();
                writer.Save(rset, "temp.srx");

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet rset2 = new SparqlResultSet();
                parser.Load(rset2, "temp.srx");

                rset.Trim();
                Console.WriteLine("Original Results");
                TestTools.ShowResults(rset);
                Console.WriteLine();

                rset2.Trim();
                Console.WriteLine("Serializes and Parsed Results");
                TestTools.ShowResults(rset2);
                Console.WriteLine();

                Assert.AreEqual(rset, rset2, "Result Sets should be equal");
            }
            else
            {
                Assert.Fail("Query did not return a Result Set as expected");
            }
        }

        [TestMethod]
        public void WritingSparqlXmlWithSpecialCharacters()
        {
            TripleStore store = new TripleStore();
            store.Add(new Graph());
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/graph");
            INode subj = g.CreateBlankNode();
            INode pred = g.CreateUriNode(new Uri("http://example.org/predicate"));
            INode obj1 = g.CreateLiteralNode("with & ampersand");
            INode obj2 = g.CreateLiteralNode("with < tags > ");
            g.Assert(subj, pred, obj1);
            g.Assert(subj, pred, obj2);
            store.Add(g);

            Object results = store.ExecuteQuery("SELECT * WHERE { ?s ?p ?o }");
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                SparqlXmlWriter writer = new SparqlXmlWriter();
                writer.Save(rset, "temp.srx");

                SparqlXmlParser parser = new SparqlXmlParser();
                SparqlResultSet rset2 = new SparqlResultSet();
                parser.Load(rset2, "temp.srx");

                rset.Trim();
                Console.WriteLine("Original Results");
                TestTools.ShowResults(rset);
                Console.WriteLine();

                rset2.Trim();
                Console.WriteLine("Serializes and Parsed Results");
                TestTools.ShowResults(rset2);
                Console.WriteLine();

                Assert.AreEqual(rset, rset2, "Result Sets should be equal");
            }
            else
            {
                Assert.Fail("Query did not return a Result Set as expected");
            }
        }
    }
}
