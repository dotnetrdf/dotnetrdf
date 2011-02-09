using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage;
using VDS.RDF.Test.Storage;

namespace VDS.RDF.Test
{
    [TestClass]
    public class NativeStoreTests : BaseTest
    {
        [TestMethod]
        public void NativeGraph()
        {
            try
            {
                //Load in our Test Graph
                TurtleParser ttlparser = new TurtleParser();
                Graph g = new Graph();
                ttlparser.Load(g, "Turtle.ttl");

                Console.WriteLine("Loaded Test Graph OK");
                Console.WriteLine("Test Graph contains:");

                Assert.IsFalse(g.IsEmpty, "Test Graph should be non-empty");

                foreach (Triple t in g.Triples)
                {
                    Console.WriteLine(t.ToString());
                }
                Console.WriteLine();

                //Create our Native Managers
                List<IGenericIOManager> managers = new List<IGenericIOManager>() {
                    new MicrosoftSqlStoreManager("localhost", "dotnetrdf_experimental","example","password"),
                    new VirtuosoManager("localhost", 1111, "DB", VirtuosoTest.VirtuosoTestUsername, VirtuosoTest.VirtuosoTestPassword)
                };

                //Save the Graph to each Manager
                foreach (IGenericIOManager manager in managers)
                {
                    Console.WriteLine("Saving using '" + manager.GetType().ToString() + "'");
                    manager.SaveGraph(g);
                    Console.WriteLine("Saved OK");
                    Console.WriteLine();
                }

                //Load Back from each Manager
                foreach (IGenericIOManager manager in managers)
                {
                    Console.WriteLine("Loading using '" + manager.GetType().ToString() + "' with a NativeGraph");
                    StoreGraph native = new StoreGraph(g.BaseUri, manager);
                    Console.WriteLine("Loaded OK");

                    Assert.IsFalse(native.IsEmpty, "Retrieved Graph should contain Triples");
                    Assert.AreEqual(g.Triples.Count, native.Triples.Count, "Retrieved Graph should contain same number of Triples as original Graph");

                    Console.WriteLine("Loaded Graph contains:");
                    foreach (Triple t in native.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();

                    native.Dispose();
                }
            }
            catch (RdfStorageException storeEx)
            {
                TestTools.ReportError("Storage Error", storeEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }
    }
}
