/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using NUnit.Framework;
using VDS.RDF.Parsing;

namespace VDS.RDF
{
    /// <summary>
    /// Abstract set of tests that can be used to test any <see cref="IGraph" /> implementation
    /// </summary>
    [TestFixture]
    public abstract class AbstractGraphTests
    {
        /// <summary>
        /// Method which derived tests should implement to provide a fresh instance that can be used for testing
        /// </summary>
        /// <returns></returns>
        protected abstract IGraph GetInstance();

        [Test]
        public void GraphIsEmpty01()
        {
            IGraph g = this.GetInstance();
            Assert.IsTrue(g.IsEmpty);
        }

        [Test]
        public void GraphIsEmpty02()
        {
            IGraph g = this.GetInstance();

            g.Assert(new Triple(g.CreateBlankNode(), g.CreateBlankNode(), g.CreateBlankNode()));
            Assert.IsFalse(g.IsEmpty);
        }

        [Test]
        public void GraphAssert01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.IsFalse(g.IsEmpty);
            Assert.IsTrue(g.ContainsTriple(t));
        }

        [Test]
        public void GraphRetract01()
        {
            IGraph g = this.GetInstance();
            g.NamespaceMap.AddNamespace(String.Empty, UriFactory.Create("http://example/"));

            Triple t = new Triple(g.CreateUriNode(":s"), g.CreateUriNode(":p"), g.CreateBlankNode(":o"));
            g.Assert(t);
            Assert.IsFalse(g.IsEmpty);
            Assert.IsTrue(g.ContainsTriple(t));

            g.Retract(t);
            Assert.IsTrue(g.IsEmpty);
            Assert.IsFalse(g.ContainsTriple(t));
        }

        [Test]
        public void GraphRetract02()
        {
            IGraph g = this.GetInstance();
            g.LoadFromEmbeddedResource("VDS.RDF.Configuration.configuration.ttl");

            Assert.IsFalse(g.IsEmpty);

            INode rdfType = g.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
            Assert.IsTrue(g.GetTriplesWithPredicate(rdfType).Any());

            g.Retract(g.GetTriplesWithPredicate(rdfType).ToList());
            Assert.IsFalse(g.GetTriplesWithPredicate(rdfType).Any());
        }

        [Fact]
        public void GetUriNode_ShouldReturnIfUsedAsObject()
        {
            // given
            var graph = GetInstance();
            var uri = new Uri("http://example.com/obj");
            var node = graph.CreateUriNode(uri);
            graph.Assert(
                node,
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                graph.CreateBlankNode());

            // when
            var uriNode = graph.GetUriNode(uri);

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetUriNode_ShouldReturnIfUsedAsSubject()
        {
            // given
            var graph = GetInstance();
            var uri = new Uri("http://example.com/subj");
            var node = graph.CreateUriNode(uri);
            graph.Assert(
                graph.CreateBlankNode(),
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                node);

            // subj
            var uriNode = graph.GetUriNode(uri);

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetUriNode_ShouldReturnNullWhenNotFound()
        {
            // given
            var graph = GetInstance();

            // subj
            var uriNode = graph.GetUriNode(new Uri("http://no/such/node"));

            // then
            Assert.Null(uriNode);
        }

        [Fact]
        public void GetBlankNode_ShouldReturnIfUsedAsSubject()
        {
            // given
            var graph = GetInstance();
            var node = graph.CreateBlankNode("xyz");
            graph.Assert(
                graph.CreateBlankNode(),
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                node);

            // subj
            var uriNode = graph.GetBlankNode("xyz");

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetBlankNode_ShouldReturnIfUsedAsObject()
        {
            // given
            var graph = GetInstance();
            var node = graph.CreateBlankNode("xyz");
            graph.Assert(
                node,
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                graph.CreateBlankNode());

            // subj
            var uriNode = graph.GetBlankNode("xyz");

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetBlankNode_ShouldReturnNullWhenNotFound()
        {
            // given
            var graph = GetInstance();

            // subj
            var uriNode = graph.GetBlankNode("other_id");

            // then
            Assert.Null(uriNode);
        }

        [Fact]
        public void GetLiteralNode_Plain_ShouldReturnWhenFound()
        {
            // given
            var graph = GetInstance();
            var literal = "test";
            var node = graph.CreateLiteralNode(literal);
            graph.Assert(
                graph.CreateBlankNode(),
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                node);

            // subj
            var uriNode = graph.GetLiteralNode(literal);

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetLiteralNode_WithLanguageTag_ShouldReturnWhenFound()
        {
            // given
            var graph = GetInstance();
            var literal = "test";
            var tag = "de";
            var node = graph.CreateLiteralNode(literal, tag);
            graph.Assert(
                graph.CreateBlankNode(),
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                node);

            // subj
            var uriNode = graph.GetLiteralNode(literal, tag);

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetLiteralNode_WithDatatype_ShouldReturnWhenFound()
        {
            // given
            var graph = GetInstance();
            var literal = "test";
            var type = new Uri("http://data/type");
            var node = graph.CreateLiteralNode(literal, type);
            graph.Assert(
                graph.CreateBlankNode(),
                graph.CreateUriNode(new Uri("http://example.com/pred")),
                node);

            // subj
            var uriNode = graph.GetLiteralNode(literal, type);

            // then
            Assert.Same(node, uriNode);
        }

        [Fact]
        public void GetLiteralNode_ShouldReturnNullWhenNotFound()
        {
            // given
            var graph = GetInstance();

            // subj
            var uriNode = graph.GetLiteralNode("something else");

            // then
            Assert.Null(uriNode);
        }
    }

#if !NO_RWLOCK // No ThreadSafeGraph
    [TestFixture]
    public class GraphTests
        : AbstractGraphTests
    {
        private const int GetNodeTestGraphSize = 100000;

        protected override IGraph GetInstance()
        {
            return new Graph();
        }

        [Fact]
        public void GetUriNode_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetUriNode(new Uri($"http://example.org/{random.Next(GetNodeTestGraphSize)}"));
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }

        [Fact]
        public void GetUriNode_WhenSelectedNodeIsNotInGraph_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetUriNode(new Uri("http://example.org/not/in/graph"));
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }

        [Fact]
        public void GetLiteralNode_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateLiteralNode($"test {random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetLiteralNode($"test {random.Next(GetNodeTestGraphSize)}");
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }

        [Fact]
        public void GetLiteralNode_WhenSelectedNodeIsNotInGraph_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateLiteralNode($"test {random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetLiteralNode("x y z");
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }

        [Fact]
        public void GetBlankNode_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateBlankNode($"{random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetBlankNode($"{random.Next(GetNodeTestGraphSize)}");
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }

        [Fact]
        public void GetBlankNode_WhenSelectedNodeIsNotInGraph_ShouldBeFastForLargeGraphs()
        {
            // given
            var total = new TimeSpan();
            var random = new Random();
            var graph = GetInstance();
            graph.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));

            for (int i1 = 0; i1 < GetNodeTestGraphSize; i1++)
            {
                graph.Assert(graph.CreateUriNode($"ex:{random.Next(GetNodeTestGraphSize)}"), graph.CreateUriNode("ex:p"), graph.CreateBlankNode($"{random.Next(GetNodeTestGraphSize)}"));
            }

            // when
            for (int i = 0; i < 1000; i++)
            {
                var stopwatch = Stopwatch.StartNew();
                graph.GetBlankNode("other_blank_id");
                total += stopwatch.Elapsed;
            }

            // then
            Assert.True(total.TotalMilliseconds < 100);
        }
    }

    [TestFixture]
    public class ThreadSafeGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new ThreadSafeGraph();
        }
    }

    [TestFixture]
    public class NonIndexedGraphTests
        : AbstractGraphTests
    {
        protected override IGraph GetInstance()
        {
            return new NonIndexedGraph();
        }
    }
#endif

}
