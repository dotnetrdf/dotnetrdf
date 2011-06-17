using System;
using System.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Storage.Virtualisation;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Storage.Test
{
    [TestClass]
    public class AdoStoreVirtualRdf
    {
        [TestMethod]
        public void StorageVirtualAdoMicrosoftGetAndCreateNodeIDs()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");

            //First generate some Nodes
            Graph g = new Graph();

            Uri now = new Uri("http://example.org/temp#" + DateTime.Now.ToString("yyyyMMddhhmmssffff"));
            List<INode> nodes = new List<INode>()
            {
                g.CreateUriNode(now),
                DateTime.Now.ToLiteral(g),
                g.CreateLiteralNode(now.GetSha256Hash()),
                g.CreateLiteralNode(now.GetSha256Hash(), new Uri(XmlSpecsHelper.XmlSchemaDataTypeString)),
                g.CreateBlankNode(),
                g.CreateBlankNode(),
                g.CreateBlankNode("_:1")
            };

            Stopwatch timer = new Stopwatch();
            foreach (INode n in nodes)
            {
                Console.WriteLine("Testing Node " + n.ToString());

                //Check that it is in the store (should be false)
                int id = n.NodeType != NodeType.Blank ? manager.GetID(n) : manager.GetBlankNodeID((IBlankNode)n);
                Console.WriteLine("ID is " + id);
                Assert.AreEqual(manager.NullID, id, "Should not be in the store at this point");

                //Now get its ID asking for one to be created
                timer.Start();
                id = n.NodeType != NodeType.Blank ? manager.GetID(n, true) : manager.GetBlankNodeID((IBlankNode)n, true);
                timer.Stop();
                Console.WriteLine("ID is " + id + ", took " + timer.Elapsed);
                Assert.AreNotEqual(manager.NullID, id, "Should be in the store at this point");
                timer.Reset();

                //Now try getting its value from the ID
                timer.Start();
                INode m = manager.GetValue(g, id);
                timer.Stop();
                long lookupTime = timer.ElapsedMilliseconds;
                Console.WriteLine("Retrieved Node " + m.ToString() + ", took " + timer.Elapsed);
                if (n.NodeType != NodeType.Blank)
                {
                    Assert.AreEqual(n, m, "Nodes should be equal after round trip to the store");
                }
                else
                {
                    Assert.AreNotEqual(n, m, "Blank Nodes should be non-equal after a round trip to the store");
                }
                timer.Reset();

                //Repeat the above to check it got cached properly
                timer.Start();
                INode m2 = manager.GetValue(g, id);
                timer.Stop();
                long cacheLookupTime = timer.ElapsedMilliseconds;
                Console.WriteLine("Retrieved Node " + m2.ToString() + ", took " + timer.Elapsed);
                if (n.NodeType != NodeType.Blank)
                {
                    Assert.AreEqual(n, m2, "Nodes should be equal after round trip to the store");
                }
                else
                {
                    Assert.AreNotEqual(n, m, "Blank Nodes should be non-equal after a round trip to the store");
                }
                Assert.AreEqual(m, m2, "Nodes retrieved from same ID should be equal");
                Assert.IsTrue(cacheLookupTime <= lookupTime, "Cached Lookup should be faster than database lookup");
                timer.Reset();

                Console.WriteLine();
            }

            manager.Dispose();
        }

        [TestMethod]
        public void StorageVirtualAdoMicrosoftLoadGraph()
        {
            MicrosoftAdoManager manager = new MicrosoftAdoManager("adostore", "example", "password");
            Graph g = new Graph();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");
            g.BaseUri = new Uri("http://example.org/virtualGraph");

            manager.SaveGraph(g);

            Stopwatch timer = new Stopwatch();
            Graph h = new Graph();
            timer.Start();
            manager.LoadGraphVirtual(h, new Uri("http://example.org/virtualGraph"));
            timer.Stop();
            Console.WriteLine("Loaded " + h.Triples.Count + " Virtual Triples in " + timer.Elapsed);
            Console.WriteLine();
            Assert.AreEqual(g.Triples.Count, h.Triples.Count, "Triple Counts should be equal");
            timer.Reset();

            //Show Virtual Triples
            foreach (Triple t in h.Triples)
            {
                int s = ((IVirtualNode<int, int>)t.Subject).VirtualID;
                int p = ((IVirtualNode<int, int>)t.Predicate).VirtualID;
                int o = ((IVirtualNode<int, int>)t.Object).VirtualID;

                Console.WriteLine(s + " " + p + " " + o + " .");
            }
            Console.WriteLine();

            //Show Materialised Triples
            NTriplesFormatter formatter = new NTriplesFormatter();
            timer.Start();
            foreach (Triple t in h.Triples)
            {
                Console.WriteLine(t.ToString(formatter));
            }
            timer.Stop();
            Console.WriteLine();
            Console.WriteLine("Took " + timer.Elapsed + " to materialise and print triples");

            manager.Dispose();
        }
    }
}
