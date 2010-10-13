using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using MongoDB;
using VDS.Alexandria;
using VDS.Alexandria.Documents;
using VDS.Alexandria.Indexing;
using VDS.Alexandria.Utilities;

namespace alexandria_tests
{
    [TestClass]
    public class IndexingTests
    {

        #region File System Store

        [TestMethod]
        public void FSIndexSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have returned some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void FSIndexPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void FSIndexSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectPredicate(fordFiesta, rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void FSIndexSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode car = g.CreateUriNode("eg:Car");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectObject(fordFiesta, car);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void FSIndexPartialEnumerate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts.Skip(5).Take(5))
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Skip(5).Take(5).Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void FSIndexRepeat()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<StreamReader,TextWriter> wrapper = new TestWrapper<StreamReader,TextWriter>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        #endregion

        #region MongoDB Store - Graph Centric

        [TestMethod]
        public void MongoGCIndexSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            //INode fordFiesta = g.GetBlankNode("autos1");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                if (t.GraphUri != null)
                {
                    Console.WriteLine(t.ToString() + " from Graph " + t.GraphUri.ToString());
                }
                else
                {
                    Console.WriteLine(t.ToString() + " from Default Graph");
                }
            }

            Assert.IsTrue(ts.Any(), "Should have returned some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoGCIndexPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoGCIndexSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectPredicate(fordFiesta, rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoGCIndexSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode car = g.CreateUriNode("eg:Car");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectObject(fordFiesta, car);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoGCIndexPartialEnumerate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts.Skip(5).Take(5))
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Skip(5).Take(5).Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoGCIndexRepeat()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        #endregion

        #region MongoDB Store - Triple Centric

        [TestMethod]
        public void MongoTCIndexSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            //INode fordFiesta = g.GetBlankNode("autos1");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                if (t.GraphUri != null)
                {
                    Console.WriteLine(t.ToString() + " from Graph " + t.GraphUri.ToString());
                }
                else
                {
                    Console.WriteLine(t.ToString() + " from Default Graph");
                }
            }

            Assert.IsTrue(ts.Any(), "Should have returned some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoTCIndexPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoTCIndexSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectPredicate(fordFiesta, rdfType);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoTCIndexSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            UriNode car = g.CreateUriNode("eg:Car");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubjectObject(fordFiesta, car);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoTCIndexPartialEnumerate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode rdfType = g.CreateUriNode("rdf:type");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithPredicate(rdfType);
            foreach (Triple t in ts.Skip(5).Take(5))
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Skip(5).Take(5).Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoTCIndexRepeat()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(new MongoDBDocumentManager(MongoDBHelper.GetConfiguration(), TestTools.GetNextStoreID(), "tc", MongoDBSchemas.TripleCentric));
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper<Document, Document> wrapper = new TestWrapper<Document, Document>(manager);
            UriNode fordFiesta = g.CreateUriNode("eg:FordFiesta");
            IEnumerable<Triple> ts = wrapper.IndexManager.GetTriplesWithSubject(fordFiesta);
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }
            foreach (Triple t in ts)
            {
                Console.WriteLine(t.ToString());
            }

            Assert.IsTrue(ts.Any(), "Should have contained some Triples");

            manager.Dispose();
        }

        #endregion
    }
}
