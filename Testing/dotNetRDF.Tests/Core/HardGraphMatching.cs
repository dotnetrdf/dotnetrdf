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
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF
{
    /// <summary>
    /// Some Tests for Hard Graph Matching based on the types of tests that Jena uses
    /// </summary>

    public class HardGraphMatching
    {
        private const int Runs = 5;
        private const int Dimension = 3;

        private const int CycleNodes = 100;
        private const int CycleDropNodes = 25;
        private const int StarNodes = 50;

        private NodeFactory _factory = new NodeFactory();
        private INodeFormatter _formatter = new NTriplesFormatter();

        [Fact]
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

                Assert.True(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i+1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [Fact]
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

                Assert.True(g.Equals(h), "Graphs should be equal");
                Console.WriteLine("Run #" + (i + 1) + " passed OK");

                g = new Graph();
                h = new Graph();
            }
        }

        [Fact]
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

                Assert.Equal(g, h);
                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }
        }

        [Fact]
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

                Assert.Equal(g, h);
                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }
        }

        [Fact]
        public void GraphHardMatchCyclic3()
        {
            Console.WriteLine("This test is just to verify that our Cyclic Graph generation is working properly");
            IGraph g = this.GenerateCyclicGraph(CycleNodes, 74, CycleDropNodes);
            IGraph h = this.GenerateCyclicGraph(CycleNodes, 90, CycleDropNodes);

            Assert.Equal(g, h);
        }

        [Fact]
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

                Assert.Equal(g, h);

                Console.WriteLine("Run #" + (i + 1) + " Passed");
            }

        }

        [Fact]
        public void GraphMatchTrivial1()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources/turtle11-unofficial/test-13.ttl");
            Graph h = new Graph();
            h.LoadFromFile("resources/turtle11-unofficial/test-13.out", new NTriplesParser());

            GraphDiffReport report = g.Difference(h);
            if (!report.AreEqual) TestTools.ShowDifferences(report);
            Assert.True(report.AreEqual);
        }

        [Fact]
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
            Assert.True(report.AreEqual);
        }

        [Fact]
        public void GraphHardTrivial3()
        {
            Graph g = new Graph();
            g.LoadFromFile("resources/turtle11/first.ttl");
            Graph h = new Graph();
            h.LoadFromFile("resources/turtle11/first.ttl");

            Assert.Equal(g, h);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase1()
        {
            const string testGraphName = "case1";
            TestGraphMatch(testGraphName);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase2()
        {
            const string testGraphName = "case2";
            TestGraphMatch(testGraphName);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase3()
        {
            const string testGraphName = "case3";
            TestGraphMatch(testGraphName);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase4()
        {
            const string testGraphName = "case4";
            TestGraphMatch(testGraphName);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase5()
        {
            const string testGraphName = "case5";
            TestGraphMatch(testGraphName);
        }

        [Fact]
        public void GraphMatchSlowOnEqualGraphsCase6()
        {
            const string testGraphName = "case6";
            TestGraphMatch(testGraphName);
        }

        private static void TestGraphMatch(string testGraphName)
        {
            Graph a = new Graph();
            a.LoadFromFile(string.Format("resources\\diff_cases\\{0}_a.ttl", testGraphName));
            Graph b = new Graph();
            b.LoadFromFile(string.Format("resources\\diff_cases\\{0}_b.ttl", testGraphName));

            Assert.True(a.Equals(b));
            Assert.True(b.Equals(a));
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

        [Fact]
        public void GraphMatchBruteForce1()
        {
            Dictionary<INode, INode> empty = new Dictionary<INode, INode>();
            INode a = this._factory.CreateBlankNode("a");
            INode b1 = this._factory.CreateBlankNode("b1");
            INode b2 = this._factory.CreateBlankNode("b2");

            //For this test we have a single blank node with two possible mappings
            Dictionary<INode, List<INode>> possibles = new Dictionary<INode, List<INode>>();
            possibles.Add(a, new List<INode> { b1, b2 });

            List<Dictionary<INode, INode>> generated = GraphMatcher.GenerateMappings(empty, possibles).ToList();
            this.PrintMappings(generated);
            Assert.Equal(2, generated.Count);
            Assert.True(generated.All(m => m.ContainsKey(a)));
            Assert.False(generated.All(m => m[a].Equals(b1)));
        }

        [Fact]
        public void GraphMatchBruteForce2()
        {
            Dictionary<INode, INode> empty = new Dictionary<INode, INode>();
            INode a1 = this._factory.CreateBlankNode("a1");
            INode a2 = this._factory.CreateBlankNode("a2");
            INode b1 = this._factory.CreateBlankNode("b1");
            INode b2 = this._factory.CreateBlankNode("b2");

            //For this test we have a two blank nodes with two possible mappings
            Dictionary<INode, List<INode>> possibles = new Dictionary<INode, List<INode>>();
            possibles.Add(a1, new List<INode> { b1, b2 });
            possibles.Add(a2, new List<INode> { b1, b2 });

            List<Dictionary<INode, INode>> generated = GraphMatcher.GenerateMappings(empty, possibles).ToList();
            this.PrintMappings(generated);
            Assert.Equal(4, generated.Count);
            Assert.True(generated.All(m => m.ContainsKey(a1)));
        }

        [Fact]
        public void GraphMatchBruteForce3()
        {
            Dictionary<INode, INode> empty = new Dictionary<INode, INode>();
            INode a1 = this._factory.CreateBlankNode("a1");
            INode a2 = this._factory.CreateBlankNode("a2");
            INode b1 = this._factory.CreateBlankNode("b1");
            INode b2 = this._factory.CreateBlankNode("b2");

            //For this test we have a two blank nodes where the first has a single mapping and the second two possible mappings
            Dictionary<INode, List<INode>> possibles = new Dictionary<INode, List<INode>>();
            possibles.Add(a1, new List<INode> { b1 });
            possibles.Add(a2, new List<INode> { b1, b2 });

            List<Dictionary<INode, INode>> generated = GraphMatcher.GenerateMappings(empty, possibles).ToList();
            this.PrintMappings(generated);
            Assert.Equal(2, generated.Count);
            Assert.True(generated.All(m => m.ContainsKey(a1)));
        }

        [Fact]
        public void GraphMatchBruteForce4()
        {
            Dictionary<INode, INode> baseMapping = new Dictionary<INode, INode>();
            INode a1 = this._factory.CreateBlankNode("a1");
            INode a2 = this._factory.CreateBlankNode("a2");
            INode b1 = this._factory.CreateBlankNode("b1");
            INode b2 = this._factory.CreateBlankNode("b2");

            //For this test we have a two blank nodes where the first has a single mapping and the second two possible mappings
            //Our base mapping also already calls out the confirmed mapping
            Dictionary<INode, List<INode>> possibles = new Dictionary<INode, List<INode>>();
            possibles.Add(a1, new List<INode> { b1 });
            possibles.Add(a2, new List<INode> { b1, b2 });
            baseMapping.Add(a1, b1);

            List<Dictionary<INode, INode>> generated = GraphMatcher.GenerateMappings(baseMapping, possibles).ToList();
            this.PrintMappings(generated);
            Assert.Equal(2, generated.Count);
            Assert.True(generated.All(m => m.ContainsKey(a1)));
        }

        private void PrintMappings(List<Dictionary<INode, INode>> mappings)
        {
            for (int i = 0; i < mappings.Count; i++)
            {
                Console.WriteLine("Mapping " + (i + 1) + " of " + mappings.Count);
                foreach (KeyValuePair<INode, INode> kvp in mappings[i])
                {
                    Console.WriteLine(this._formatter.Format(kvp.Key) + " => " + this._formatter.Format(kvp.Value));
                }
                Console.WriteLine();
            }
        }

        [Fact]
        public void GraphMatchNull1()
        {
            GraphMatcher matcher = new GraphMatcher();
            Assert.True(matcher.Equals(null, null));
        }

        [Fact]
        public void GraphMatchNull2()
        {
            GraphMatcher matcher = new GraphMatcher();
            Assert.False(matcher.Equals(new Graph(), null));
        }

        [Fact]
        public void GraphMatchNull3()
        {
            GraphMatcher matcher = new GraphMatcher();
            Assert.False(matcher.Equals(null, new Graph()));
        }

        [Fact]
        public void GraphMatchNull4()
        {
            IGraph g = new Graph();
            Assert.False(g.Equals((IGraph)null));
        }
    }
}
