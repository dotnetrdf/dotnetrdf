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

namespace VDS.RDF
{
    using System;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;
    using Xunit;

    public partial class WrapperNodeTests
    {
        [Fact]
        public void Requires_underlying_node()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new MockWrapperNode(null));
        }

        [Fact]
        public void Delegates_Equals_object()
        {
            var node = new NodeFactory().CreateBlankNode();
            var nodeObject = node as object;
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(nodeObject);
            var actual = wrapper.Equals(nodeObject);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_GetHashCode()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.GetHashCode();
            var actual = wrapper.GetHashCode();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_ToString()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.ToString();
            var actual = wrapper.ToString();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_NodeType()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.NodeType;
            var actual = wrapper.NodeType;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Graph()
        {
            var node = new Graph().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Graph;
            var actual = wrapper.Graph;

            Assert.Same(expected, actual);
        }

        [Fact]
        public void Delegates_GraphUri()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            wrapper.GraphUri = UriFactory.Create("http://example.com/");

            var expected = node.GraphUri;
            var actual = wrapper.GraphUri;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_CompareTo_node()
        {
            var node = new NodeFactory().CreateBlankNode() as INode;
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates__CompareTo_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates__CompareTo_graphLiteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates__CompareTo_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates__CompareTo_uri()
        {
            var node = new NodeFactory().CreateUriNode(UriFactory.Create("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates__CompareTo_variable()
        {
            var node = new NodeFactory().CreateVariableNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_node()
        {
            var node = new NodeFactory().CreateBlankNode() as INode;
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_graphLiteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_uri()
        {
            var node = new NodeFactory().CreateUriNode(UriFactory.Create("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equal_variable()
        {
            var node = new NodeFactory().CreateVariableNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_ToString_formatter()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.ToString(new CsvFormatter());
            var actual = wrapper.ToString(new CsvFormatter());

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_ToString_formatter_segment()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var formatter = new CsvFormatter();

            var expected = node.ToString(formatter, TripleSegment.Subject);
            var actual = wrapper.ToString(formatter, TripleSegment.Subject);

            Assert.Equal(expected, actual);
        }
    }
}