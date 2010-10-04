using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test
{
    [TestClass]
    public class BasicTests2 : BaseTest
    {
        [TestMethod]
        public void GraphEquality() {
            try
            {
                Console.WriteLine("Going to get two copies of a Graph from DBPedia and compare");
                Console.WriteLine("Using the DBPedia Graph for Barack Obama");

                Graph g = new Graph();
                Graph h = new Graph();
                Uri target = new Uri("http://dbpedia.org/resource/Barack_Obama");

                VDS.RDF.Parsing.UriLoader.Load(g, target);
                Console.WriteLine("Loaded first copy OK - " + g.Triples.Count + " Triples");
                VDS.RDF.Parsing.UriLoader.Load(h, target);
                Console.WriteLine("Loaded second copy OK - " + h.Triples.Count + " Triples");

                //Should have same Base Uri
                Assert.AreEqual(g.BaseUri, h.BaseUri, "Should have the same Base URI after being loaded from the same URI via the URILoader");

                //Do equality check
                Console.WriteLine("Checking the Equality of the Graphs");
                //TestTools.CompareGraphs(g, h, true);
                Dictionary<INode, INode> mapping;
                bool equals = g.Equals(h, out mapping);
                Assert.IsTrue(equals, "Graphs should have been equal");
                if (mapping != null)
                {
                    Console.WriteLine("Blank Node Mapping was:");
                    foreach (KeyValuePair<INode, INode> pair in mapping)
                    {
                        Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                    }
                }
                Console.WriteLine();

                //Get a third graph of something different
                Console.WriteLine("Going to get a third Graph of something different and check it is non-equal");
                Uri target2 = new Uri("http://dbpedia.org/resource/Nottingham");
                Graph i = new Graph();
                VDS.RDF.Parsing.UriLoader.Load(i, target2);

                //Should have different Base URIs and be non-equal
                Assert.AreNotEqual(g.BaseUri, i.BaseUri, "Graphs retrieved from different URIs via the URILoader should have different Base URIs");
                Assert.AreNotEqual(h.BaseUri, i.BaseUri, "Graphs retrieved from different URIs via the URILoader should have different Base URIs");
                Assert.IsFalse(g.Equals(i));
                Assert.IsFalse(h.Equals(i));
                //TestTools.CompareGraphs(g, i, false);
                //TestTools.CompareGraphs(h, i, false);

            }
            catch (WebException webEx)
            {
                TestTools.ReportError("Web Exception", webEx, false);
                Console.WriteLine();
                Console.WriteLine("Unable to retrieve the Graphs from the Web successfully!");
                Assert.Inconclusive();
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Exception", parseEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void SubGraphMatching()
        {
            Graph parent = new Graph();
            FileLoader.Load(parent, "InferenceTest.ttl");
            Graph subgraph = new Graph();
            subgraph.NamespaceMap.Import(parent.NamespaceMap);
            subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));

            //Check method calls
            Dictionary<INode, INode> mapping;
            Console.WriteLine("Doing basic sub-graph matching with no BNode tests");
            Assert.IsTrue(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.IsFalse(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.IsFalse(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.IsTrue(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Add an extra triple into the Graph which will cause it to no longer be a sub-graph
            Console.WriteLine("Adding an extra Triple so the sub-graph is no longer such");
            subgraph.Assert(new Triple(subgraph.CreateUriNode("eg:Rocket"), subgraph.CreateUriNode("rdf:type"), subgraph.CreateUriNode("eg:AirVehicle")));
            Assert.IsFalse(parent.HasSubGraph(subgraph, out mapping), "Sub-graph should no longer be considered a sub-graph");
            Assert.IsFalse(subgraph.IsSubGraphOf(parent, out mapping), "Sub-graph should no longer be considered a sub-graph");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Reset the sub-graph
            Console.WriteLine("Resetting the sub-graph");
            subgraph = new Graph();
            subgraph.NamespaceMap.Import(parent.NamespaceMap);
            subgraph.Assert(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));
            Console.WriteLine("Adding additional information to the parent Graph, this should not affect the fact that the sub-graph is a sub-graph of it");
            Assert.IsTrue(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.IsFalse(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.IsFalse(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.IsTrue(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Remove stuff from parent graph so it won't match any more
            Console.WriteLine("Removing stuff from parent graph so that it won't have the sub-graph anymore");
            parent.Retract(parent.GetTriplesWithSubject(parent.CreateUriNode("eg:FordFiesta")));
            Assert.IsFalse(parent.HasSubGraph(subgraph, out mapping), "Parent should no longer contian the sub-graph");
            Assert.IsFalse(subgraph.IsSubGraphOf(parent, out mapping), "Parent should no longer contain the sub-graph");
            Console.WriteLine("OK");
            Console.WriteLine();
        }

        [TestMethod]
        public void SubGraphMatchingWithBNodes()
        {
            Graph parent = new Graph();
            FileLoader.Load(parent, "Turtle.ttl");
            Graph subgraph = new Graph();
            subgraph.Assert(parent.Triples.Where(t => !t.IsGroundTriple));

            //Check method calls
            Dictionary<INode, INode> mapping;
            Console.WriteLine("Doing basic sub-graph matching with BNode tests");
            Assert.IsTrue(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.IsFalse(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.IsFalse(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.IsTrue(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            //Eliminate some of the Triples from the sub-graph
            Console.WriteLine("Eliminating some Triples from the sub-graph and seeing if the mapping still computes OK");
            subgraph.Retract(subgraph.Triples.Skip(2).Take(5));
            Assert.IsTrue(parent.HasSubGraph(subgraph, out mapping), "Failed to match the sub-graph as expected");
            Assert.IsFalse(parent.IsSubGraphOf(subgraph, out mapping), "Parent should not be a sub-graph of the sub-graph");
            Assert.IsFalse(subgraph.HasSubGraph(parent, out mapping), "Sub-graph should not have parent as its sub-graph");
            Assert.IsTrue(subgraph.IsSubGraphOf(parent, out mapping), "Failed to match the sub-graph as expected");
            Console.WriteLine("OK");
            Console.WriteLine();

            Console.WriteLine("Eliminating Blank Nodes from the parent Graph to check that the sub-graph is no longer considered as such afterwards");
            parent.Retract(parent.Triples.Where(t => !t.IsGroundTriple));
            Assert.IsFalse(parent.HasSubGraph(subgraph), "Sub-graph should no longer be considered as such");
            Assert.IsFalse(subgraph.IsSubGraphOf(parent), "Sub-graph should no longer be considered as such");

        }

        [TestMethod]
        public void GraphWithBNodeEquality()
        {
            try
            {
                Console.WriteLine("Testing Graph Equality when the Graphs have Blank Nodes");
                Graph g = new Graph();
                Graph h = new Graph();

                TurtleParser ttlparser = new TurtleParser();
                ttlparser.Load(g, "MergePart1.ttl");
                ttlparser.Load(h, "MergePart1.ttl");

                Assert.AreEqual(g.BaseUri, h.BaseUri, "The Base URIs of the Graphs should not be affected by the Load and so should be both null");
                //TestTools.CompareGraphs(g, h, true);
                Dictionary<INode, INode> mapping;
                bool equals = g.Equals(h, out mapping);
                Assert.IsTrue(equals, "Graphs should have been equal");
                if (mapping != null)
                {
                    Console.WriteLine("Blank Node Mapping was:");
                    foreach (KeyValuePair<INode, INode> pair in mapping)
                    {
                        Console.WriteLine(pair.Key.ToString() + " => " + pair.Value.ToString());
                    }
                }
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parsing Exception", parseEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
        }

        [TestMethod]
        public void UriLoader()
        {
            try
            {
                Options.UriLoaderCaching = false;

                List<Uri> testUris = new List<Uri>() {
                    new Uri("http://www.bbc.co.uk/programmes/b0080bbs#programme"),
                    new Uri("http://dbpedia.org/resource/Southampton"),
                    new Uri("file:///MergePart1.ttl"),
                    /*new Uri("file:///D:/PhD Work/dotNetRDF/unittest/resources/MergePart1.ttl"),*/
                    new Uri("http://www.dotnetrdf.org/configuration#")
                };

                Console.WriteLine("## URI Loader Test Suite");

                foreach (Uri u in testUris)
                {
                    Console.WriteLine("# Testing URI '" + u.ToString() + "'");

                    //Load the Test RDF
                    Graph g = new Graph();
                    Assert.IsNotNull(g);
                    VDS.RDF.Parsing.UriLoader.Load(g, u);

                    if (!u.IsFile)
                    {
                        Assert.AreEqual(u, g.BaseUri);
                    }

                    Console.WriteLine();

                    Console.WriteLine("Following Triples were generated");
                    foreach (Triple t in g.Triples)
                    {
                        Console.WriteLine(t.ToString());
                    }
                    Console.WriteLine();
                }

            }
            catch (WebException webEx)
            {
                TestTools.ReportError("HTTP Error", webEx, true);
            }
            catch (RdfParseException parseEx)
            {
                TestTools.ReportError("Parser Error", parseEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Other Error", ex, true);
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [TestMethod]
        public void EqualityOperator()
        {
            Console.WriteLine("Testing that the overridden operators for Nodes work as expected");

            try
            {
                Graph g = new Graph();
                BlankNode a = g.CreateBlankNode();
                BlankNode b = g.CreateBlankNode();

                Console.WriteLine("Testing using Equals() method");
                Assert.IsFalse(a.Equals(b), "Two different Blank Nodes should be non-equal");
                Assert.IsTrue(a.Equals(a), "A Blank Node should be equal to itself");
                Assert.IsTrue(b.Equals(b), "A Blank Node should be equal to itself");
                Console.WriteLine("OK");

                Console.WriteLine();

                Console.WriteLine("Testing using == operator");
                Assert.IsFalse(a == b, "Two different Blank Nodes should be non-equal");
                Assert.IsTrue(a == a, "A Blank Node should be equal to itself");
                Assert.IsTrue(b == b, "A Blank Node should be equal to itself");
                Console.WriteLine("OK");

                Console.WriteLine();

                //Test typed as INode
                INode c = g.CreateBlankNode();
                INode d = g.CreateBlankNode();

                Console.WriteLine("Now testing with typed as INode using Equals()");
                Assert.IsFalse(c.Equals(d), "Two different Nodes should be non-equal");
                Assert.IsTrue(c.Equals(c), "A Node should be equal to itself");
                Assert.IsTrue(d.Equals(d), "A Node should be equal to itself");
                Console.WriteLine("OK");

                Console.WriteLine();

                Console.WriteLine("Now testing with typed as INode using == operator");
                Assert.IsFalse(c == d, "Two different Nodes should be non-equal");
                Assert.IsTrue(c == c, "A Node should be equal to itself");
                Assert.IsTrue(d == d, "A Node should be equal to itself");
                Console.WriteLine("OK");

            }
            catch (RdfException rdfEx)
            {
                TestTools.ReportError("RDF Error", rdfEx, true);
            }
            catch (Exception ex)
            {
                TestTools.ReportError("Error", ex, true);
            }
        }
    }
}
