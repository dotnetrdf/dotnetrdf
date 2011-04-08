using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Test
{
    /// <summary>
    /// Some Tests for Hard Graph Matching based on the types of tests that Jena uses
    /// </summary>
    [TestClass]
    public class HardGraphMatching
    {
        private const int Quantity = 20;
        private const int Dimension = 6;

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

            for (int i = 0; i < Quantity; i++)
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

            for (int i = 0; i < Quantity; i++)
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
        public void GraphHardMatch3()
        {
            IGraph g = new Graph();
            IGraph h = new Graph();

            int size = 1 << Dimension;
            Random rnd = new Random();

            for (int i = 0; i < Quantity; i++)
            {
                int a = rnd.Next(size);
                int b = rnd.Next(size);
                while (a == b)
                {
                    b = rnd.Next(size);
                }

                Hypercube hc1 = new Hypercube(Dimension, g);
                hc1 = hc1.Duplicate(a).Duplicate(a).Duplicate(a);
                Hypercube hc2 = new Hypercube(Dimension, h);
                hc2 = hc2.Duplicate(b).Duplicate(b).Duplicate(b);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    TestTools.ShowGraph(h);
                }

                Assert.IsFalse(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [TestMethod]
        public void GraphHardMatch4()
        {
            IGraph g = new Graph();
            IGraph h = new Graph();

            int size = 1 << Dimension;
            Random rnd = new Random();

            for (int i = 0; i < Quantity; i++)
            {
                int a = rnd.Next(size);
                int b = rnd.Next(size);
                while (a == b)
                {
                    b = rnd.Next(size);
                }

                Hypercube hc1 = new Hypercube(Dimension, g);
                hc1 = hc1.Toggle(a, b);
                Hypercube hc2 = new Hypercube(Dimension, h);
                hc2 = hc2.Toggle(b, a);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    TestTools.ShowGraph(h);
                }

                Assert.IsFalse(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [TestMethod]
        public void GraphHardMatch5()
        {
            IGraph g = new Graph();
            IGraph h = new Graph();

            int size = 1 << Dimension;
            Random rnd = new Random();

            for (int i = 0; i < Quantity; i++)
            {
                int a = rnd.Next(size);
                int b = rnd.Next(size);
                while (a == b)
                {
                    b = rnd.Next(size);
                }

                Hypercube hc1 = new Hypercube(Dimension, g);
                hc1 = hc1.Toggle(a, b);
                Hypercube hc2 = new Hypercube(Dimension, h);
                hc2 = hc2.Toggle(b, a);

                if (i == 0)
                {
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    TestTools.ShowGraph(h);
                }

                Assert.IsFalse(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [TestMethod]
        public void GraphHardMatchCyclic()
        {
            Random rnd = new Random();

            for (int i = 0; i < Quantity; i++)
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

            for (int i = 0; i < Quantity; i++)
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
            for (int i = 0; i < Quantity; i++)
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
