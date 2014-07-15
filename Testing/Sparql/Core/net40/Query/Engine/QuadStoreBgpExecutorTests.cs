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
        private IGraph CreateGraph()
        {
            IGraph g = new Graph();
            g.Namespaces.AddNamespace(String.Empty, new Uri("http://example.org/"));

            INode s1 = g.CreateUriNode(":subject");
            INode s2 = g.CreateBlankNode();
            INode p = g.CreateUriNode(":predicate");
            INode o1 = g.CreateUriNode(":object");
            INode o2 = g.CreateLiteralNode("test");
            INode o3 = g.CreateBlankNode();

            g.Assert(s1, p, o1);
            g.Assert(s1, p, o2);
            g.Assert(s1, p, o3);
            g.Assert(s2, p, o1);
            g.Assert(s2, p, o2);
            g.Assert(s2, p, o3);

            return g;
        }

        private IQuadStore CreateQuadStore(IGraph g)
        {
            GraphStore gs = new GraphStore();
            gs.Add(g);
            return gs;
        }

        [Test]
        public void QuadStoreBgpExecutorGround1()
        {
            IGraph g = this.CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateUriNode(":object"));

            List<ISet> results = executor.Match(Quad.DefaultGraphNode, search).ToList();
            Assert.AreEqual(1, results.Count);
            Assert.AreEqual(0, results.First().Variables.Count());
        }

        [Test]
        public void QuadStoreBgpExecutorGround2()
        {
            IGraph g = this.CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateUriNode(":nosuchthing2"));

            List<ISet> results = executor.Match(Quad.DefaultGraphNode, search).ToList();
            Assert.AreEqual(0, results.Count);
        }

        [Test]
        public void QuadStoreBgpExecutorMatch1()
        {
            IGraph g = this.CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateUriNode(":subject"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));

            List<ISet> results = executor.Match(Quad.DefaultGraphNode, search).ToList();
            Assert.AreEqual(3, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 1));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o")));
        }

        [Test]
        public void QuadStoreBgpExecutorMatch2()
        {
            IGraph g = this.CreateGraph();
            IQuadStore qs = CreateQuadStore(g);
            IBgpExecutor executor = new QuadStoreBgpExecutor(qs);

            Triple search = new Triple(g.CreateVariableNode("s"), g.CreateUriNode(":predicate"), g.CreateVariableNode("o"));

            List<ISet> results = executor.Match(Quad.DefaultGraphNode, search).ToList();
            Assert.AreEqual(6, results.Count);
            Assert.IsTrue(results.All(s => s.Variables.Count() == 2));
            Assert.IsTrue(results.All(s => s.ContainsVariable("o") && s.ContainsVariable("s")));
        }
    }
}
