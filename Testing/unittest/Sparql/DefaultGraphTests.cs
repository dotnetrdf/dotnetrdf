using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
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

        [TestMethod]
        public void SparqlDefaultGraphExists3()
        {
            SqlDataset dataset = new SqlDataset(new MicrosoftSqlStoreManager("localhost", "unit_test", "example", "password"));

            //Create Default Graph only if required
            if (!dataset.HasGraph(null))
            {
                Graph g = new Graph();
                g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                dataset.AddGraph(g);
                dataset.Flush();
            }

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("ASK WHERE { GRAPH ?g { ?s ?p ?o }}");
            LeviathanQueryProcessor lvn = new LeviathanQueryProcessor(dataset);
            Object results = lvn.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }

            dataset.Flush();
        }

        [TestMethod]
        public void SparqlDefaultGraphExists4()
        {
            SqlDataset dataset = new SqlDataset(new MicrosoftSqlStoreManager("localhost", "unit_test", "example", "password"));

            //Create Default Graph only if required
            if (!dataset.HasGraph(null))
            {
                Graph g = new Graph();
                g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                dataset.AddGraph(g);
                dataset.Flush();
            }

            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("ASK WHERE { GRAPH <dotnetrdf:default-graph> { ?s ?p ?o }}");
            LeviathanQueryProcessor lvn = new LeviathanQueryProcessor(dataset);
            Object results = lvn.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }

            dataset.Flush();
        }

        [TestMethod]
        public void SparqlDefaultGraphExists5()
        {
            MicrosoftSqlStoreManager db = new MicrosoftSqlStoreManager("localhost", "unit_test", "example", "password");
            db.PreserveState = true;
            SqlTripleStore store = new SqlTripleStore(db);

            //Create Default Graph only if required
            if (!store.HasGraph(null))
            {
                Graph g = new Graph();
                g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                store.Add(g);
                store.Flush();
            }

            Object results = store.ExecuteQuery("ASK WHERE { GRAPH ?g { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }

            store.Flush();
            db.PreserveState = false;
            store.Dispose();
        }

        [TestMethod]
        public void SparqlDefaultGraphExists6()
        {
            MicrosoftSqlStoreManager db = new MicrosoftSqlStoreManager("localhost", "unit_test", "example", "password");
            db.PreserveState = true;
            SqlTripleStore store = new SqlTripleStore(db);

            //Create Default Graph only if required
            if (!store.HasGraph(null))
            {
                Graph g = new Graph();
                g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
                store.Add(g);
                store.Flush();
            }

            Object results = store.ExecuteQuery("ASK WHERE { GRAPH <dotnetrdf:default-graph> { ?s ?p ?o }}");
            if (results is SparqlResultSet)
            {
                Assert.IsTrue(((SparqlResultSet)results).Result);
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }

            store.Flush();
            db.PreserveState = false;
            store.Dispose();
        }
    }
}
