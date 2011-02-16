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
        private const int StarNodes = 50;

        [TestMethod]
        public void HardGraphMatch1()
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
        public void HardGraphMatch2()
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
        public void HardGraphMatch3()
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
        public void HardGraphMatch4()
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
        public void HardGraphMatch5()
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
        public void HardGraphMatchCyclic()
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
        public void HardGraphMatchStar()
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
            Graph g = new Graph();
            UriNode rdfValue = g.CreateUriNode("rdf:value");

            if (seed >= nodes) seed = nodes - 2;

            List<BlankNode> bnodes = new List<BlankNode>(nodes);
            for (int i = 0; i < nodes; i++)
            {
                bnodes.Add(g.CreateBlankNode());
            }

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
            UriNode rdfValue = g.CreateUriNode("rdf:value");

            List<BlankNode> bnodes = new List<BlankNode>();
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
