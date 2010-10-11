using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MongoDB;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.Alexandria;
using VDS.Alexandria.Datasets;

namespace alexandria_tests
{
    [TestClass]
    public class SparqlTests
    {

        #region File System Store

        [TestMethod]
        public void FSSparqlSelectSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s a ?o }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p <http://example.org/vehicles/Car> }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> a ?o }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p <http://example.org/vehicles/Car> }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectPredicateObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s a <http://example.org/vehicles/Car> }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectAll()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Now load another Graph and add it
            Graph h = new Graph();
            FileLoader.Load(h, "test.nt");

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.SaveGraph(h);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void FSSparqlSelectMultipleTriplePatterns()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE { ?s a ex:Car ; ex:Speed ?speed }");
            AlexandriaFileDataset dataset = new AlexandriaFileDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        #endregion

        #region MongoDB Store

        [TestMethod]
        public void MongoSparqlSelectSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p ?o }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s a ?o }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p <http://example.org/vehicles/Car> }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> a ?o }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { <http://example.org/vehicles/FordFiesta> ?p <http://example.org/vehicles/Car> }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectPredicateObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s a <http://example.org/vehicles/Car> }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectAll()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Now load another Graph and add it
            Graph h = new Graph();
            FileLoader.Load(h, "test.nt");

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.SaveGraph(h);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("SELECT * WHERE { ?s ?p ?o }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        [TestMethod]
        public void MongoSparqlSelectMultipleTriplePatterns()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);
            manager.Flush();

            //Create and Evaluate the Query
            SparqlQueryParser parser = new SparqlQueryParser();
            SparqlQuery q = parser.ParseFromString("PREFIX ex: <http://example.org/vehicles/> SELECT * WHERE { ?s a ex:Car ; ex:Speed ?speed }");
            AlexandriaMongoDBDataset dataset = new AlexandriaMongoDBDataset(manager);

            Object results = q.Evaluate(dataset);

            manager.Dispose();

            if (results is SparqlResultSet)
            {
                SparqlResultSet rset = (SparqlResultSet)results;
                foreach (SparqlResult r in rset)
                {
                    Console.WriteLine(r.ToString());
                }

                Assert.IsTrue(rset.Count > 0, "Expected some results from the Query");
            }
            else
            {
                Assert.Fail("Expected a Result Set from the Query");
            }
        }

        #endregion
    }
}
