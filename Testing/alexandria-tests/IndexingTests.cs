using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Alexandria;
using Alexandria.Documents;
using Alexandria.Indexing;

namespace alexandria_tests
{
    [TestClass]
    public class IndexingTests
    {
        [TestMethod]
        public void IndexSubject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
        public void IndexPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
        public void IndexSubjectPredicate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
        public void IndexSubjectObject()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
        public void IndexPartialEnumerate()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
        public void IndexRepeat()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            Thread.Sleep(500);

            //Try and access an index from the Store
            TestWrapper wrapper = new TestWrapper(manager);
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
    }
}
