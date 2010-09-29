using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using Alexandria;

namespace alexandria_tests
{
    [TestClass]
    public class BasicIOTests
    {
        [TestMethod]
        public void SaveLoadGraphTest()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            g.BaseUri = null;

            //Open an Alexandria Store and save the Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
            manager.SaveGraph(g);

            //Try and read the Graph back from the Store
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should have been equal");

            //manager.Dispose();
        }

        [TestMethod]
        public void AppendTriplesTest()
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
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
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
        public void DeleteTriplesTest()
        {
            //Load in our Test Graph
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");

            //Get the Triples that we want to delete later
            List<Triple> ts = g.GetTriplesWithSubject(g.CreateUriNode("eg:FordFiesta")).ToList();

            //Open an Alexandria Store and save the original Graph
            AlexandriaFileManager manager = new AlexandriaFileManager("test");
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
    }
}
