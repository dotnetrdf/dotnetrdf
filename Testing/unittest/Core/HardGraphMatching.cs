/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Some Tests for Hard Graph Matching based on the types of tests that Jena uses
    /// </summary>
    [TestClass]
    public class HardGraphMatching
    {
        private const int Runs = 5;
        private const int Dimension = 3;

        private const int CycleNodes = 100;
        private const int CycleDropNodes = 25;
        private const int StarNodes = 50;

        [TestMethod]
        public void GraphHardMatch1()
        {
            IGraph g = new Graph();
            IGraph h = new Graph();

            int size = 1 << Dimension;
            Random rnd = new Random();

            for (int i = 0; i < Runs; i++)
            {
                int a = rnd.Next(size);

                Hypercube hc1 = new Hypercube(Dimension, g);
                hc1 = hc1.Duplicate(a).Duplicate(a).Duplicate(a);
                Hypercube hc2 = new Hypercube(Dimension, h);
                hc2 = hc2.Duplicate(a).Duplicate(a).Duplicate(a);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    TestTools.ShowGraph(h);
                }

                Assert.IsTrue(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i+1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [TestMethod]
        public void GraphHardMatch2()
        {
            IGraph g = new Graph();
            IGraph h = new Graph();

            int size = 1 << Dimension;
            Random rnd = new Random();

            for (int i = 0; i < Runs; i++)
            {
                int a = rnd.Next(size);

                DiHypercube hc1 = new DiHypercube(Dimension, g);
                hc1 = hc1.Duplicate(a).Duplicate(a).Duplicate(a);
                DiHypercube hc2 = new DiHypercube(Dimension, h);
                hc2 = hc2.Duplicate(a).Duplicate(a).Duplicate(a);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    TestTools.ShowGraph(h);
                }

                Assert.IsTrue(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [TestMethod]
        public void GraphHardMatchCyclic()
        {
            Random rnd = new Random();

            for (int i = 0; i < Runs; i++)
            {
                IGraph g = this.GenerateCyclicGraph(CycleNodes, rnd.Next(CycleNodes));
                IGraph h = this.GenerateCyclicGraph(CycleNodes, rnd.Next(CycleNodes));

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    TestTools.ShowGraph(h);
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }
        }

        [TestMethod]
        public void GraphHardMatchCyclic2()
        {
            Random rnd = new Random();

            for (int i = 0; i < Runs; i++)
            {
                IGraph g = this.GenerateCyclicGraph(CycleNodes, rnd.Next(CycleNodes), CycleDropNodes);
                IGraph h = this.GenerateCyclicGraph(CycleNodes, rnd.Next(CycleNodes), CycleDropNodes);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    TestTools.ShowGraph(h);
                }

                Assert.AreEqual(g, h, "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }
        }

        [TestMethod]
        public void GraphHardMatchCyclic3()
        {
            Console.WriteLine("This test is just to verify that our Cyclic Graph generation is working properly");
            IGraph g = this.GenerateCyclicGraph(CycleNodes, 74, CycleDropNodes);
            IGraph h = this.GenerateCyclicGraph(CycleNodes, 90, CycleDropNodes);

            Assert.AreEqual(g, h, "Graphs should be equal");
        }

        [TestMethod]
        public void GraphHardMatchStar()
        {
            for (int i = 0; i < Runs; i++)
            {
                IGraph g = this.GenerateStarGraph(StarNodes);
                IGraph h = this.GenerateStarGraph(StarNodes);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    TestTools.ShowGraph(h);
                }

                Assert.AreEqual(g, h, "Graphs should have been equal");

                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }

        }

        [TestMethod]
        public void GraphMatchTrivial1()
        {
            Graph g = new Graph();
            g.LoadFromFile("turtle11/test-13.ttl");
            Graph h = new Graph();
            h.LoadFromFile("turtle11/test-13.out", new NTriplesParser());

            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual) TestTools.ShowDifferences(report);
            Assert.IsTrue(report.AreEqual);
        }

        [TestMethod]
        public void GraphMatchTrivial2()
        {
            Graph g = new Graph();
            IBlankNode a = g.CreateBlankNode("b1");
            IBlankNode b = g.CreateBlankNode("b2");
            IBlankNode c = g.CreateBlankNode("b3");
            INode pred = g.CreateUriNode(UriFactory.Create("http://predicate"));

            g.Assert(a, pred, g.CreateLiteralNode("A"));
            g.Assert(a, pred, b);
            g.Assert(b, pred, g.CreateLiteralNode("B"));
            g.Assert(b, pred, c);
            g.Assert(c, pred, g.CreateLiteralNode("C"));
            g.Assert(c, pred, a);

            Graph h = new Graph();
            IBlankNode a2 = h.CreateBlankNode("b4");
            IBlankNode b2 = h.CreateBlankNode("b5");
            IBlankNode c2 = h.CreateBlankNode("b3");
            INode pred2 = h.CreateUriNode(UriFactory.Create("http://predicate"));

            h.Assert(a2, pred2, h.CreateLiteralNode("A"));
            h.Assert(a2, pred2, b2);
            h.Assert(b2, pred2, h.CreateLiteralNode("B"));
            h.Assert(b2, pred2, c2);
            h.Assert(c2, pred2, h.CreateLiteralNode("C"));
            h.Assert(c2, pred2, a2);

            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual) TestTools.ShowDifferences(report);
            Assert.IsTrue(report.AreEqual);
        }

        private IGraph GenerateCyclicGraph(int nodes, int seed)
        {
            return GenerateCyclicGraph(nodes, seed, 0);
        }

        private IGraph GenerateCyclicGraph(int nodes, int seed, int toDrop)
        {
            Console.WriteLine("Generating Cyclic Graph - Nodes " + nodes + " - Seed " + seed + " - Drop " + toDrop);

            Graph g = new Graph();
            IUriNode rdfValue = g.CreateUriNode("rdf:value");

            if (seed >= nodes - toDrop - 1) seed = nodes - toDrop - 2;

            List<IBlankNode> bnodes = new List<IBlankNode>(nodes);
            for (int i = 0; i < nodes; i++)
            {
                bnodes.Add(g.CreateBlankNode());
            }

            Random rnd = new Random();
            for (int i = 0; i < toDrop; i++)
            {
                bnodes.RemoveAt(rnd.Next(bnodes.Count));
            }

            if (seed >= bnodes.Count - 1) seed = bnodes.Count - 2;

            Console.WriteLine("Seed value is " + seed);

            //Generate a cycle of Triples starting from the seed
            int counter = 0;
            while (counter <= bnodes.Count)
            {
                g.Assert(bnodes[seed], rdfValue, bnodes[seed + 1]);
                counter++;
                seed++;

                if (seed == bnodes.Count - 1)
                {
                    g.Assert(bnodes[seed], rdfValue, bnodes[0]);
                    seed = 0;
                }
            }

            return g;
        }

        private IGraph GenerateStarGraph(int nodes)
        {
            Graph g = new Graph();
            IUriNode rdfValue = g.CreateUriNode("rdf:value");

            List<IBlankNode> bnodes = new List<IBlankNode>();
            for (int i = 0; i < nodes; i++)
            {
                bnodes.Add(g.CreateBlankNode());
            }

            for (int i = 0; i < bnodes.Count; i++)
            {
                for (int j = 0; j < bnodes.Count; j++)
                {
                    if (i == j) continue;
                    g.Assert(bnodes[i], rdfValue, bnodes[j]);
                }
            }

            return g;
        }
    }
}
