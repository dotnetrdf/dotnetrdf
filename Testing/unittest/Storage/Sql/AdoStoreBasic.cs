using System;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Configuration;
using VDS.RDF.Parsing;
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
        public void StorageAdoMicrosoftLoadGraph()
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
        public void StorageAdoMicrosoftLoadDefaultGraph()
        {
            this.StorageAdoMicrosoftSaveDefaultGraph();

            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Graph g = new Graph();
            manager.LoadGraph(g, (Uri)null);

            NTriplesFormatter formatter = new NTriplesFormatter();
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }

            Assert.IsFalse(g.IsEmpty, "Graph should not be empty");

            Graph orig = new Graph();
            orig.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            Assert.AreEqual(orig, g, "Graphs should be equal");

            manager.Dispose();
        }

        [TestMethod]
        public void StorageAdoMicrosoftSaveGraph()
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

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageAdoMicrosoftSaveDefaultGraph()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = null;

            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            manager.SaveGraph(g);
            timer.Stop();

            Console.WriteLine("Write Time - " + timer.Elapsed);

            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void StorageAdoMicrosoftSaveGraph2()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/adoStore/savedGraph");
            for (int i = 1; i <= 25000; i++)
            {
                INode n = g.CreateUriNode(new Uri("http://example.org/" + i));
                g.Assert(n, n, n);
            }
            Console.WriteLine("Generated Graph has " + g.Triples.Count + " Triples");

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

        //[TestMethod]
        //public void StorageAdoMicrosoftSaveGraph3()
        //{
        //    Graph g = new Graph();
        //    g.BaseUri = new Uri("http://example.org/adoStore/savedGraph");
        //    g.LoadFromFile("dataset_250.ttl");
        //    Console.WriteLine("BSBM Graph has " + g.Triples.Count + " Triples");
        //    Debug.WriteLine("BSBM Graph has " + g.Triples.Count);

        //    MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
        //    Stopwatch timer = new Stopwatch();
        //    timer.Start();
        //    manager.SaveGraph(g);
        //    timer.Stop();

        //    Console.WriteLine("Write Time - " + timer.Elapsed);
        //    Debug.WriteLine("Write Time - " + timer.Elapsed);

        //    Graph h = new Graph();
        //    timer.Reset();
        //    timer.Start();
        //    manager.LoadGraph(h, g.BaseUri);
        //    timer.Stop();

        //    Console.WriteLine("Read Time - " + timer.Elapsed);
        //    Debug.WriteLine("Read Time - " + timer.Elapsed);

        //    Assert.AreEqual(g, h, "Graphs should be equal");

        //    manager.Dispose();
        //}

        [TestMethod]
        public void StorageAdoMicrosoftListGraphs()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");

            foreach (Uri u in manager.ListGraphs())
            {
                if (u != null)
                {
                    Console.WriteLine(u.ToString());
                }
                else
                {
                    Console.WriteLine("Default Graph");
                }
            }

            manager.Dispose();
        }

        [TestMethod]
        public void StorageAdoMicrosoftDeleteGraph()
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

            //Now delete the Graph
            manager.DeleteGraph(g.BaseUri);
            Graph i = new Graph();
            manager.LoadGraph(i, g.BaseUri);

            Assert.IsTrue(i.IsEmpty, "Graph should be empty as was deleted from the store");
            Assert.AreNotEqual(g, i, "Graphs should not be equal");
            Assert.AreNotEqual(h, i, "Graphs should not be equal");
        }

        [TestMethod]
        public void StorageAdoMicrosoftUpdateGraph()
        {
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/adoStore/updatedGraph");

            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Stopwatch timer = new Stopwatch();
            timer.Start();
            manager.SaveGraph(g);
            timer.Stop();

            Console.WriteLine("Write Time - " + timer.Elapsed);

            //Now delete a certain set of Triples
            INode rdfType = g.CreateUriNode("rdf:type");
            manager.UpdateGraph(g.BaseUri, null, g.GetTriplesWithPredicate(rdfType));

            //Load the Graph and check it doesn't contain and rdf:type triples
            Graph h = new Graph();
            manager.LoadGraph(h, g.BaseUri);

            Assert.IsFalse(h.GetTriplesWithPredicate(rdfType).Any(), "Loaded Graph should not contain any rdf:type triples");
            Assert.IsTrue(g.HasSubGraph(h), "Loaded Graph should be a sub-graph of the original graph");

            //Now add some triples
            Graph i = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Query.Optimisation.OptimiserStats.ttl");
            manager.UpdateGraph(g.BaseUri, i.Triples, null);

            //Load Graph again and check it contains the relevant triples
            h = new Graph();
            manager.LoadGraph(h, g.BaseUri);
            Assert.IsTrue(h.HasSubGraph(i), "Loaded Graph should have the added Triples as a sub-graph");

            manager.Dispose();
        }

        [TestMethod]
        public void StorageAdoMicrosoftConfigSerialization()
        {
            NTriplesFormatter formatter = new NTriplesFormatter();
            try
            {
                MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
                Assert.IsNotNull(manager);

                Graph g = new Graph();
                INode rdfType = g.CreateUriNode(new Uri(RdfSpecsHelper.RdfType));
                INode dnrType = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.PropertyType);
                INode objFactory = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.ClassObjectFactory);
                INode adoFactory = g.CreateLiteralNode("VDS.RDF.Configuration.AdoObjectFactory, dotNetRDF.Data.Sql");
                INode genericManager = ConfigurationLoader.CreateConfigurationNode(g, ConfigurationLoader.ClassGenericManager);
                INode mssqlManager = g.CreateLiteralNode("VDS.RDF.Storage.MicrosoftAdoManager, dotNetRDF.Data.Sql");

                //Serialize Configuration
                ConfigurationSerializationContext context = new ConfigurationSerializationContext(g);
                manager.SerializeConfiguration(context);

                Console.WriteLine("Serialized Configuration");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                //Ensure that it was serialized
                INode factory = g.GetTriplesWithPredicateObject(rdfType, objFactory).Select(t => t.Subject).FirstOrDefault();
                Assert.IsNotNull(factory, "Should be an object factory in the serialized configuration");
                Assert.IsTrue(g.ContainsTriple(new Triple(factory, dnrType, adoFactory)), "Should contain a Triple declaring the dnr:type to be the ADO Object factory type");
                INode objNode = g.GetTriplesWithPredicateObject(rdfType, genericManager).Select(t => t.Subject).FirstOrDefault();
                Assert.IsNotNull(objNode, "Should be a generic manager in the serialized configuration");
                Assert.IsTrue(g.ContainsTriple(new Triple(objNode, dnrType, mssqlManager)), "Should contain a Triple declaring the dnr:type to be the Microsoft ADO Manager type");

                //Serialize again
                manager.SerializeConfiguration(context);

                Console.WriteLine("Serialized Configuration (after 2nd pass)");
                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString(formatter));
                }
                Console.WriteLine();

                //Ensure that object factory has not been serialized again 
                Assert.AreEqual(1, g.GetTriplesWithPredicateObject(rdfType, objFactory).Count(), "Should only be 1 Object Factory registered even after a 2nd serializer pass");

                //Now try to load the object
                ConfigurationLoader.AutoDetectObjectFactories(g);
                Object loadedObj = ConfigurationLoader.LoadObject(g, objNode);
                if (loadedObj is MicrosoftAdoManager)
                {
                    Assert.AreEqual(manager.ToString(), loadedObj.ToString(), "String forms should be equal");
                }
                else
                {
                    Assert.Fail("Returned an object of type '" + loadedObj.GetType().FullName + "' when deserializing");
                }
            }
            catch (DotNetRdfConfigurationException configEx)
            {
                TestTools.ReportError("Config Error", configEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }
    }
}
