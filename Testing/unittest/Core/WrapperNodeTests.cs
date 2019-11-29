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

using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml;
using System.Xml.Serialization;

namespace VDS.RDF
{
    using System;
    using VDS.RDF.Writing;
    using VDS.RDF.Writing.Formatting;
    using Xunit;

    public class WrapperNodeTests
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
        public void Delegates_CompareTo_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_CompareTo_graphLiteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_CompareTo_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_CompareTo_uri()
        {
            var node = new NodeFactory().CreateUriNode(UriFactory.Create("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_CompareTo_variable()
        {
            var node = new NodeFactory().CreateVariableNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.CompareTo(node);
            var actual = wrapper.CompareTo(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_node()
        {
            var node = new NodeFactory().CreateBlankNode() as INode;
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_blank()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_graphLiteral()
        {
            var node = new NodeFactory().CreateGraphLiteralNode();
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_literal()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_uri()
        {
            var node = new NodeFactory().CreateUriNode(UriFactory.Create("http://example.com/"));
            var wrapper = new MockWrapperNode(node);

            var expected = node.Equals(node);
            var actual = wrapper.Equals(node);

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Delegates_Equals_variable()
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

        [Fact]
        public void Fails_invalid_InternalID()
        {
            var node = new NodeFactory().CreateLiteralNode(string.Empty);
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<InvalidCastException>(() =>
                ((IBlankNode)wrapper).InternalID);
        }

        [Fact]
        public void Delegates_InternalID()
        {
            var expected = new Guid().ToString();
            var node = new NodeFactory().CreateBlankNode(expected);
            var wrapper = new MockWrapperNode(node);

            var actual = ((IBlankNode)wrapper).InternalID;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_invalid_Uri()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<InvalidCastException>(() =>
                ((IUriNode)wrapper).Uri);
        }

        [Fact]
        public void Delegates_Uri()
        {
            var expected = UriFactory.Create("urn:s");
            var node = new NodeFactory().CreateUriNode(expected);
            var wrapper = new MockWrapperNode(node);

            var actual = ((IUriNode)wrapper).Uri;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_invalid_Value()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<InvalidCastException>(() =>
                ((ILiteralNode)wrapper).Value);
        }

        [Fact]
        public void Delegates_Value()
        {
            var expected = string.Empty;
            var node = new NodeFactory().CreateLiteralNode(expected);
            var wrapper = new MockWrapperNode(node);

            var actual = ((ILiteralNode)wrapper).Value;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_invalid_Language()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<InvalidCastException>(() =>
                ((ILiteralNode)wrapper).Language);
        }

        [Fact]
        public void Delegates_Language()
        {
            var expected = "en";
            var node = new NodeFactory().CreateLiteralNode(string.Empty, expected);
            var wrapper = new MockWrapperNode(node);

            var actual = ((ILiteralNode)wrapper).Language;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Fails_invalid_DataType()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);

            Assert.Throws<InvalidCastException>(() =>
                ((ILiteralNode)wrapper).DataType);
        }

        [Fact]
        public void Delegates_DataType()
        {
            var expected = UriFactory.Create("urn:s");
            var node = new NodeFactory().CreateLiteralNode(string.Empty, expected);
            var wrapper = new MockWrapperNode(node);

            var actual = ((ILiteralNode)wrapper).DataType;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Doesnt_implement_GetObjectData()
        {
            var node = new NodeFactory().CreateBlankNode();
            var wrapper = new MockWrapperNode(node);
            var serializer = new BinaryFormatter(null, default(StreamingContext));

            using (var stream = new MemoryStream())
            {
                Assert.Throws<NotImplementedException>(() =>
                    serializer.Serialize(stream, wrapper));
            }
        }

        [Fact]
        public void Doesnt_implement_GetSchema()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.GetSchema());
        }

        [Fact]
        public void Doesnt_implement_ReadXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.ReadXml(XmlReader.Create(Stream.Null)));
        }

        [Fact]
        public void Doesnt_implement_WriteXml()
        {
            var node = new NodeFactory().CreateBlankNode();
            IXmlSerializable wrapper = new MockWrapperNode(node);

            Assert.Throws<NotImplementedException>(() =>
                wrapper.WriteXml(XmlWriter.Create(Stream.Null)));
        }
    }
}