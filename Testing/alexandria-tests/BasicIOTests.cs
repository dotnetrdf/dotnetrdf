using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Writing;
using Alexandria;
using Alexandria.Documents;

namespace alexandria_tests
{
    [TestClass]
    public class BasicIOTests
    {

        #region File System Store

        [TestMethod]
        public void FSSaveLoadGraph()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSSaveLoadWithoutDispose()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");
        }

        [TestMethod]
        public void FSSaveLoadGraphWithDifferentManagers()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            String storeID = TestTools.GetNextStoreID();
            AlexandriaFileManager manager = new AlexandriaFileManager(storeID);
            manager.SaveGraph(g);
            manager.Dispose();

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager = new AlexandriaFileManager(storeID);
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSSaveOverwriteGraph()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            //Now load another Graph and overwrite the first Graph
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");
            i.BaseUri = null;

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);
            Graph j = new Graph();
            manager.LoadGraph(j, i.BaseUri);

            Assert.AreEqual(i, j, "Graphs should have been equal");
            Assert.AreNotEqual(g, i, "Graphs should not be equal");
            Assert.AreNotEqual(h, j, "Graphs should not be equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSSaveMultipleGraphs()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            //Now load another Graph and add it
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);
            Graph j = new Graph();
            manager.LoadGraph(j, i.BaseUri);

            Assert.AreEqual(i, j, "Graphs should have been equal");
            Assert.AreNotEqual(g, i, "Graphs should not be equal");
            Assert.AreNotEqual(h, j, "Graphs should not be equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSAppendTriples()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the Triples that we want to add later
            UriNode spaceShuttle = g.CreateUriNode("eg:SpaceShuttle");
            UriNode rdfType = g.CreateUriNode("rdf:type");
            UriNode airVehicle = g.CreateUriNode("eg:AirVehicle");
            Triple[] ts = new Triple[] { new Triple(spaceShuttle, rdfType, airVehicle) };

            //Open an Alexandria Store and save the original Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Append the Triples both locally and to the store
            g.Assert(ts);
            manager.UpdateGraph(g.BaseUri, ts, null);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void FSDeleteTriples()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the Triples that we want to delete later
            List<Triple> ts = g.GetTriplesWithSubject(g.CreateUriNode("eg:FordFiesta")).ToList();

            //Open an Alexandria Store and save the original Graph
            AlexandriaFileManager manager = new AlexandriaFileManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Remove the Triples both locally and to the store
            g.Retract(ts);
            manager.UpdateGraph(g.BaseUri, null, ts);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        #endregion

        #region MongoDB Store

        [TestMethod]
        public void MongoSaveLoadGraph()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoSaveLoadGraphWithDifferentManagers()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            String storeID = TestTools.GetNextStoreID();
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(storeID);
            manager.SaveGraph(g);
            manager.Dispose();

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager = new AlexandriaMongoDBManager(storeID);
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoSaveOverwriteGraph()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal (1)");

            //Now load another Graph and overwrite the first Graph
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");
            i.BaseUri = null;

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);
            Graph j = new Graph();
            manager.LoadGraph(j, i.BaseUri);

            Assert.AreEqual(i, j, "Graphs should have been equal (2)");
            Assert.AreNotEqual(g, i, "Graphs should not be equal (3)");
            Assert.AreNotEqual(h, j, "Graphs should not be equal (4)");

            manager.Dispose();
        }

        [TestMethod]
        public void MongoSaveMultipleGraphs()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Open an Alexandria Store and save the Graph
            AlexandriaMongoDBManager manager = new AlexandriaMongoDBManager(TestTools.GetNextStoreID());
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Console.WriteLine(StringWriter.Write(g, new NTriplesWriter()));
            Console.WriteLine();
            Console.WriteLine(StringWriter.Write(h, new NTriplesWriter()));

            Assert.AreEqual(g, h, "Graphs should have been equal (1)");
            Console.WriteLine();

            //Now load another Graph and add it
            Graph i = new Graph();
            FileLoader.Load(i, "test.nt");

            //Save it to the Store which should overwrite it and then load it back
            manager.SaveGraph(i);
            Graph j = new Graph();
            manager.LoadGraph(j, i.BaseUri);

            Console.WriteLine(StringWriter.Write(i, new NTriplesWriter()));
            Console.WriteLine();
            Console.WriteLine(StringWriter.Write(j, new NTriplesWriter()));

            Assert.AreEqual(i, j, "Graphs should have been equal (2)");
            Assert.AreNotEqual(g, i, "Graphs should not be equal (3)");
            Assert.AreNotEqual(h, j, "Graphs should not be equal (4)");

            manager.Dispose();
        }

        #endregion
    }
}
