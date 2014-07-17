using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NUnit.Framework.Constraints;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine.Medusa;

namespace VDS.RDF.Query.Engine
{
    [TestFixture]
    public class QuadStoreBgpExecutorTests
    {
        private static IGraph CreateGraph()
        {
            IGraph g = new Graph();
            g.Namespaces.AddNamespace(String.Empty, new Uri("http://example.org/"));

            INode s1 = g.CreateUriNode(":subject");
            INode s2 = g.CreateBlankNode();
            INode p = g.CreateUriNode(":predicate");
            INode o1 = g.CreateUriNode(":object");
            INode o2 = g.CreateLiteralNode("test");

            g.Assert(s1, p, o1);
            g.Assert(s1, p, o2);
            g.Assert(s1, p, s2);
            g.Assert(s2, p, o1);
            g.Assert(s2, p, o2);

            return g;
        }

        private static IQuadStore CreateQuadStore(IGraph g)
        {
            GraphStore gs = new GraphStore();
            gs.Add(g);
            return gs;
        }

        [Test]
        public void QuadStoreBgpExecutorGround1()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateUriNode(":object"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0, results.First().Variables.Count());
        }

        [Test]
        public void QuadStoreBgpExecutorGround2()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateUriNode(":nosuchthing2"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void QuadStoreBgpExecutorMatch1()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 1));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o")));
        }

        [Test]
        public void QuadStoreBgpExecutorMatch2()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o") && s.ContainsVariable("s")));
        }

        [Test]
        public void QuadStoreBgpExecutorMatch3()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), g.CreateBlankNode());

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("s")));
        }

        [Test]
        public void QuadStoreBgpExecutorChainedMatch1()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));
            Triple search2 = new Triple(g.CreateVariableNode("o"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o2"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o") && s.ContainsVariable("s")));

            results = results.SelectMany(s => executor.Match(search2, s, context)).ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 3));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o2")));
        }

        [Test]
        public void QuadStoreBgpExecutorChainedMatch2()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            INode b = g.CreateBlankNode();
            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), b);
            Triple search2 = new Triple(b, g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("s")));

            results = results.SelectMany(s => executor.Match(search2, s, context)).ToList();
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 3));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o")));
        }

        [Test]
        public void QuadStoreBgpExecutorChainedMatch3()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));
            Triple search2 = new Triple(g.CreateVariableNode("o"), g.CreateUriNode(":predicate"), g.CreateLiteralNode("nosuchthing"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o") && s.ContainsVariable("s")));

            results = results.SelectMany(s => executor.Match(search2, s, context)).ToList();
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void QuadStoreBgpExecutorDistinctMatch1()
        {
            IGraph g = CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("a"), g.CreateVariableNode("b"), g.CreateVariableNode("c"));
            Triple search2 = new Triple(g.CreateVariableNode("d"), g.CreateVariableNode("e"), g.CreateVariableNode("f"));

            QueryExecutionContext context = new QueryExecutionContext(Quad.DefaultGraphNode, Quad.DefaultGraphNode.AsEnumerable(), null);
            List<ISet> results = executor.Match(search, context).ToList();
            Assert.AreEqual(5, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 3));
            Assert.IsTrue(results.All(s => s.ContainsVariable("a") && s.ContainsVariable("b") && s.ContainsVariable("c")));

            results = results.SelectMany(s => executor.Match(search2, s, context)).ToList();
            Assert.AreEqual(25, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 6));
            Assert.IsTrue(results.All(s => s.ContainsVariable("a") && s.ContainsVariable("b") && s.ContainsVariable("c") && s.ContainsVariable("d") && s.ContainsVariable("e") && s.ContainsVariable("f")));
        }
    }
}
