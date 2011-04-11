using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Datasets;
using VDS.RDF.Storage;
using VDS.RDF.Update;
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

        [TestMethod]
        public void SparqlDatasetDefaultGraphManagement()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);
            Graph h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            InMemoryDataset dataset = new InMemoryDataset(store);
            dataset.SetDefaultGraph(h);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");

            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
                Assert.IsTrue(((SparqlResultSet)results).IsEmpty, "Results should be empty as an empty Graph was set as the Default Graph");
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphManagement2()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            g.Assert(g.CreateUriNode(new Uri("http://example.org/subject")), g.CreateUriNode(new Uri("http://example.org/predicate")), g.CreateUriNode(new Uri("http://example.org/object")));
            store.Add(g);
            Graph h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            InMemoryDataset dataset = new InMemoryDataset(store);
            dataset.SetDefaultGraph(g);
            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");

            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                TestTools.ShowResults(results);
                Assert.IsFalse(((SparqlResultSet)results).IsEmpty, "Results should be false as a non-empty Graph was set as the Default Graph");
            }
            else
            {
                Assert.Fail("ASK Query did not return a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlDatasetDefaultGraphManagementWithUpdate()
        {
            TripleStore store = new TripleStore();
            Graph g = new Graph();
            store.Add(g);
            Graph h = new Graph();
            h.BaseUri = new Uri("http://example.org/someOtherGraph");
            store.Add(h);

            InMemoryDataset dataset = new InMemoryDataset(store, h.BaseUri);
            LeviathanUpdateProcessor processor = new LeviathanUpdateProcessor(dataset);
            SparqlUpdateParser parser = new SparqlUpdateParser();
            SparqlUpdateCommandSet cmds = parser.ParseFromString("LOAD <http://dbpedia.org/resource/Ilkeston>");

            processor.ProcessCommandSet(cmds);

            Assert.IsTrue(g.IsEmpty, "Graph with null URI (normally the default Graph) should be empty as the Default Graph for the Dataset should have been a named Graph so this Graph should not have been filled by the LOAD Command");
            Assert.IsFalse(h.IsEmpty, "Graph with name should be non-empty as it should have been the Default Graph for the Dataset and so filled by the LOAD Command");
        }

        [TestMethod]
        public void SparqlGraphClause()
        {
            String query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            InMemoryDataset dataset = new InMemoryDataset();
            IGraph ex = new Graph();
            FileLoader.Load(ex, "InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            dataset.SetDefaultGraph(def);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.AreEqual(ex.Triples.Count, rset.Count, "Number of Results should have been equal to number of Triples");
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGraphClause2()
        {
            String query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            InMemoryDataset dataset = new InMemoryDataset();
            IGraph ex = new Graph();
            FileLoader.Load(ex, "InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.AreEqual(ex.Triples.Count, rset.Count, "Number of Results should have been equal to number of Triples");
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGraphClause3()
        {
            String query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            InMemoryDataset dataset = new InMemoryDataset(false);
            IGraph ex = new Graph();
            FileLoader.Load(ex, "InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            dataset.SetDefaultGraph(def);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.AreEqual(ex.Triples.Count, rset.Count, "Number of Results should have been equal to number of Triples");
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }

        [TestMethod]
        public void SparqlGraphClause4()
        {
            String query = "SELECT * WHERE { GRAPH ?g { ?s ?p ?o } }";
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString(query);

            InMemoryDataset dataset = new InMemoryDataset(false);
            IGraph ex = new Graph();
            FileLoader.Load(ex, "InferenceTest.ttl");
            ex.BaseUri = new Uri("http://example.org/graph");
            dataset.AddGraph(ex);

            IGraph def = new Graph();
            dataset.AddGraph(def);

            LeviathanQueryProcessor processor = new LeviathanQueryProcessor(dataset);
            Object results = processor.ProcessQuery(q);
            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                TestTools.ShowResults(rset);
                Assert.AreEqual(ex.Triples.Count, rset.Count, "Number of Results should have been equal to number of Triples");
            }
            else
            {
                Assert.Fail("Did not get a SPARQL Result Set as expected");
            }
        }
    }
}
