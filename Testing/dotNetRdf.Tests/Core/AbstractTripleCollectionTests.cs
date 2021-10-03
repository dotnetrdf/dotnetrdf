/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2021 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
*/

using System;
using System.Linq;
using Xunit;
using FluentAssertions;

namespace VDS.RDF
{
    public abstract class AbstractTripleCollectionTests
    {
        private INodeFactory NodeFactory { get; }

        protected AbstractTripleCollectionTests()
        {
            NodeFactory = new NodeFactory(uriFactory: new CachingUriFactory());
            NodeFactory.NamespaceMap.AddNamespace("ex", new Uri("http://example.org/"));
        }

        protected abstract BaseTripleCollection GetInstance();

        [Fact]
        public void ItIndexesQuotedTripleInSubjectPosition()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var assertedTriple = new Triple(
                NodeFactory.CreateTripleNode(quotedTriple),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));
            collection.Add(assertedTriple);

            collection.ContainsAsserted(assertedTriple).Should().BeTrue();
            collection.ContainsQuoted(quotedTriple).Should().BeTrue();
            collection.ContainsQuoted(assertedTriple).Should().BeFalse();
            collection.ContainsAsserted(quotedTriple).Should().BeFalse();

            // Can look-up using the asserted triple instance wrapped in a new TripleNode
            ITripleNode testNode = NodeFactory.CreateTripleNode(quotedTriple);
            collection.WithSubject(testNode).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithSubject(testNode).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.QuotedWithSubject(testNode).Should().BeEmpty();

            // Can look-up using a new triple with the same subject, predicate and object wrapped in a new triple node
            ITripleNode testNode2 = NodeFactory.CreateTripleNode(new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            ));
            collection.WithSubject(testNode2).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithSubject(testNode2).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithSubjectPredicate(testNode, NodeFactory.CreateUriNode("ex:p")).Should()
                .ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithSubjectObject(testNode, NodeFactory.CreateUriNode("ex:o")).Should()
                .ContainSingle().Which.Should().Be(assertedTriple);
            collection.QuotedWithSubject(testNode2).Should().BeEmpty();
            collection.QuotedWithSubjectPredicate(testNode, NodeFactory.CreateUriNode("ex:p")).Should().BeEmpty();
            collection.QuotedWithSubjectObject(testNode, NodeFactory.CreateUriNode("ex:o")).Should().BeEmpty();

            // Quoted triple should also have been indexed:
            collection.WithSubject(NodeFactory.CreateUriNode("ex:s")).Should().ContainSingle().Which.Should()
                .Be(quotedTriple);
            collection.AssertedWithSubject(NodeFactory.CreateUriNode("ex:s")).Should().BeEmpty();
            collection.QuotedWithSubject(NodeFactory.CreateUriNode("ex:s")).Should().ContainSingle().Which.Should()
                .Be(quotedTriple);

            // predicate index should contain both triples
            collection.WithPredicate(NodeFactory.CreateUriNode("ex:p")).Should().Contain(assertedTriple).And
                .Contain(quotedTriple);
            collection.AssertedWithPredicate(NodeFactory.CreateUriNode("ex:p")).Should().ContainSingle().Which.Should()
                .Be(assertedTriple);
            collection.QuotedWithPredicate(NodeFactory.CreateUriNode("ex:p")).Should().ContainSingle().Which.Should()
                .Be(quotedTriple);
        }

        [Fact]
        public void ItIndexesQuotedTripleInObjectPosition()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var assertedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
                collection.Add(assertedTriple);

            collection.ContainsAsserted(assertedTriple).Should().BeTrue();
            collection.ContainsQuoted(quotedTriple).Should().BeTrue();
            collection.ContainsQuoted(assertedTriple).Should().BeFalse();
            collection.ContainsAsserted(quotedTriple).Should().BeFalse();

            // Can look-up using the asserted triple instance wrapped in a new TripleNode
            ITripleNode testNode = NodeFactory.CreateTripleNode(quotedTriple);
            collection.WithObject(testNode).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithObject(testNode).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.QuotedWithObject(testNode).Should().BeEmpty();

            // Can look-up using a new triple with the same subject, predicate and object wrapped in a new triple node
            ITripleNode testNode2 = NodeFactory.CreateTripleNode(new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            ));
            collection.WithObject(testNode2).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.AssertedWithObject(testNode2).Should().ContainSingle().Which.Should().Be(assertedTriple);
            collection.QuotedWithObject(testNode2).Should().BeEmpty();

            // Quoted triple should also be accessible by its nodes
            collection.WithObject(NodeFactory.CreateUriNode("ex:o")).Should()
                .ContainSingle().Which.Should().Be(quotedTriple);
            collection.AssertedWithObject(NodeFactory.CreateUriNode("ex:o")).Should().BeEmpty();
            collection.QuotedWithObject(NodeFactory.CreateUriNode("ex:o")).Should()
                .ContainSingle().Which.Should().Be(quotedTriple);

        }

        [Fact]
        public void ItIndexesNestedQuotedTriples()
        {
            BaseTripleCollection collection = GetInstance();
            var nestedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var quotedTriple = new Triple(
                NodeFactory.CreateTripleNode(nestedTriple),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var assertedTriple = new Triple(
                NodeFactory.CreateTripleNode(quotedTriple),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));

            collection.Add(assertedTriple);

            ITripleNode testNode = NodeFactory.CreateTripleNode(nestedTriple);
            collection.WithSubject(testNode).Should().ContainSingle().Which.Should().Be(quotedTriple);
            collection.QuotedWithSubject(testNode).Should().ContainSingle().Which.Should().Be(quotedTriple);
            collection.AssertedWithSubject(testNode).Should().BeEmpty();

            collection.Asserted.Count().Should().Be(1);
            collection.Quoted.Count().Should().Be(2);
            collection.Count.Should().Be(3);
        }

        [Fact]
        public void DeletingATripleCanDeleteQuotedTriples()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));
            var assertedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
            collection.Add(assertedTriple);
            // Before deletion we have the asserted triple and its quoted triple
            collection.Count.Should().Be(2);
            collection.Delete(assertedTriple);

            // Deleting the asserted triple also deletes the quoted triple because there are no other references to it.
            collection.Count.Should().Be(0);
        }

        [Fact]
        public void QuotedTripleIsNotDeletedIfItIsAlsoAsserted()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));
            var assertedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
            collection.Add(assertedTriple);
            collection.Add(quotedTriple);

            // Before deletion we have the asserted triple and its quoted triple
            collection.Count.Should().Be(2);

            collection.Delete(assertedTriple);

            // Deleting the asserted triple does not delete the quoted triple because it is also asserted
            collection.Count.Should().Be(1);
        }

        [Fact]
        public void QuotedTripleIsNotDeletedIfOtherReferencesToItExist()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));
            var assertedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
            var assertedTriple2 = new Triple(
                NodeFactory.CreateUriNode("ex:s2"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
            collection.Add(assertedTriple);
            collection.Add(assertedTriple2);

            // Before deletion we have the asserted triple and its quoted triple
            collection.Count.Should().Be(3);

            collection.Delete(assertedTriple);

            // Deleting the asserted triple does not delete the quoted triple because there is another reference to it
            collection.Count.Should().Be(2);

            // Deleting the second assertion will remove the quoted triple because now there are no other refs
            collection.Delete(assertedTriple2);
            collection.Count.Should().Be(0);
        }

        [Fact]
        public void NestedQuotationsAreAlsoRemoved()
        {
            BaseTripleCollection collection = GetInstance();
            var nestedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var quotedTriple = new Triple(
                NodeFactory.CreateTripleNode(nestedTriple),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o")
            );
            var assertedTriple = new Triple(
                NodeFactory.CreateTripleNode(quotedTriple),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));

            collection.Add(assertedTriple);
            collection.Count.Should().Be(3);

            // Deleting the asserted triple should delete both the directly quoted triple and the indirectly quoted one.
            collection.Delete(assertedTriple);
            collection.Count.Should().Be(0);
        }

        [Fact]
        public void DeletingAQuotedTripleHasNoEffect()
        {
            BaseTripleCollection collection = GetInstance();
            var quotedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateUriNode("ex:o"));
            var assertedTriple = new Triple(
                NodeFactory.CreateUriNode("ex:s"),
                NodeFactory.CreateUriNode("ex:p"),
                NodeFactory.CreateTripleNode(quotedTriple));
            collection.Add(assertedTriple);
            // Before deletion we have the asserted triple and its quoted triple
            collection.Count.Should().Be(2);
            var wasDeleted = collection.Delete(quotedTriple);

            // Deleting the quoted triple has no effect as it is not asserted in the graph
            wasDeleted.Should().BeFalse();
            collection.Count.Should().Be(2);
        }
    }

}
