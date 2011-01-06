using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Sparql
{
    /// <summary>
    /// Summary description for DefaultGraphTests
    /// </summary>
    [TestClass]
    public class DefaultGraphTests
    {
        [TestMethod]
        public void SparqlDefaultGraphExists()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);

            Object results = store.ExecuteQuery("ASK WHERE { GRAPH ?g { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlDefaultGraphExists2()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);

            Object results = store.ExecuteQuery("ASK WHERE { GRAPH <dotnetrdf:default-graph> { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }
        }
    }
}
