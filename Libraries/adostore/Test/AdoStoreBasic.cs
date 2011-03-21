using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Storage;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Storage
{
    [TestClass]
    public class AdoStoreBasic
    {
        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager(null, null, null, null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation2()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager(null, null, null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation3()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "user", null);
        }

        [TestMethod, ExpectedException(typeof(ArgumentNullException))]
        public void StorageAdoMicrosoftBadInstantiation4()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", null, "password");
        }

        [TestMethod]
        public void StorageAdoMicrosoftCheckVersion()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            int version = manager.CheckVersion();
            Console.WriteLine("Version: " + version);
            manager.Dispose();

            Assert.AreEqual(1, version, "Expected Version 1 to be reported");
        }

        [TestMethod]
        public void StorageAdoStoreLoadGraph()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Graph g = new Graph();
            manager.LoadGraph(g, new Uri("http://example.org/graph"));

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsTrue(g.IsEmpty, "Graph should be empty");

            manager.Dispose();
        }

        [TestMethod]
        public void StorageAdoStoreLoadGraph2()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Graph g = new Graph();
            manager.LoadGraph(g, (Uri)null);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");
            Assert.AreEqual(2, g.Triples.Count, "Expected 2 Triples");

            manager.Dispose();
        }

        [TestMethod]
        public void StorageAdoStoreSaveGraph()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/adoStore/savedGraph");

            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            manager.SaveGraph(g);
            timer.Stop();

            Console.WriteLine("Write Time - " + timer.Elapsed);

            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageAdoStoreSaveGraph2()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/adoStore/savedGraph");
            for (int i = 1; i <= 25000; i++)
            {
                INode n = g.CreateUriNode(new Uri("http://example.org/" + i));
                g.Assert(n, n, n);
            }
            Console.WriteLine("Generated Graph has " + g.Triples.Count);

            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            manager.SaveGraph(g);
            timer.Stop();

            Console.WriteLine("Write Time - " + timer.Elapsed);

            Graph h = new Graph();
            timer.Reset();
            timer.Start();
            manager.LoadGraph(h, g.BaseUri);
            timer.Stop();

            Console.WriteLine("Read Time - " + timer.Elapsed);

            Assert.AreEqual(g, h, "Graphs should be equal");
        }
    }
}
