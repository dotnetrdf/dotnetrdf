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
using VDS.RDF.Writing;
using VDS.RDF.Writing.Formatting;
using Xunit;

namespace VDS.RDF;

public class WrapperNodeTests
{
    private readonly NodeFactory _factory = new(new NodeFactoryOptions());

    [Fact]
    public void Requires_underlying_node()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new MockWrapperNode(null));
    }

    [Fact]
    public void Delegates_Equals_object()
    {
        var node = _factory.CreateBlankNode();
        var nodeObject = node as object;
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(nodeObject);
        var actual = wrapper.Equals(nodeObject);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_GetHashCode()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.GetHashCode();
        var actual = wrapper.GetHashCode();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_ToString()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.ToString();
        var actual = wrapper.ToString();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_NodeType()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.NodeType;
        var actual = wrapper.NodeType;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_node()
    {
        var node = _factory.CreateBlankNode() as INode;
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_blank()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_graphLiteral()
    {
        var node = _factory.CreateGraphLiteralNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_literal()
    {
        var node = _factory.CreateLiteralNode(string.Empty);
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_uri()
    {
        var node = _factory.CreateUriNode(UriFactory.Root.Create("http://example.com/"));
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_CompareTo_variable()
    {
        var node = _factory.CreateVariableNode(string.Empty);
        var wrapper = new MockWrapperNode(node);

        var expected = node.CompareTo(node);
        var actual = wrapper.CompareTo(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_node()
    {
        var node = _factory.CreateBlankNode() as INode;
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_blank()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_graphLiteral()
    {
        var node = _factory.CreateGraphLiteralNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_literal()
    {
        var node = _factory.CreateLiteralNode(string.Empty);
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_uri()
    {
        var node = _factory.CreateUriNode(UriFactory.Root.Create("http://example.com/"));
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_Equals_variable()
    {
        var node = _factory.CreateVariableNode(string.Empty);
        var wrapper = new MockWrapperNode(node);

        var expected = node.Equals(node);
        var actual = wrapper.Equals(node);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_ToString_formatter()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        var expected = node.ToString(new CsvFormatter());
        var actual = wrapper.ToString(new CsvFormatter());

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Delegates_ToString_formatter_segment()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);
        var formatter = new CsvFormatter();

        var expected = node.ToString(formatter, TripleSegment.Subject);
        var actual = wrapper.ToString(formatter, TripleSegment.Subject);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_invalid_InternalID()
    {
        var node = _factory.CreateLiteralNode(string.Empty);
        var wrapper = new MockWrapperNode(node);

        Assert.Throws<InvalidCastException>(() =>
            ((IBlankNode)wrapper).InternalID);
    }

    [Fact]
    public void Delegates_InternalID()
    {
        var expected = new Guid().ToString();
        var node = _factory.CreateBlankNode(expected);
        var wrapper = new MockWrapperNode(node);

        var actual = ((IBlankNode)wrapper).InternalID;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_invalid_Uri()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        Assert.Throws<InvalidCastException>(() =>
            ((IUriNode)wrapper).Uri);
    }

    [Fact]
    public void Delegates_Uri()
    {
        var expected = UriFactory.Root.Create("urn:s");
        var node = _factory.CreateUriNode(expected);
        var wrapper = new MockWrapperNode(node);

        var actual = ((IUriNode)wrapper).Uri;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_invalid_Value()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        Assert.Throws<InvalidCastException>(() =>
            ((ILiteralNode)wrapper).Value);
    }

    [Fact]
    public void Delegates_Value()
    {
        var expected = string.Empty;
        var node = _factory.CreateLiteralNode(expected);
        var wrapper = new MockWrapperNode(node);

        var actual = ((ILiteralNode)wrapper).Value;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_invalid_Language()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        Assert.Throws<InvalidCastException>(() =>
            ((ILiteralNode)wrapper).Language);
    }

    [Fact]
    public void Delegates_Language()
    {
        var expected = "en";
        var node = _factory.CreateLiteralNode(string.Empty, expected);
        var wrapper = new MockWrapperNode(node);

        var actual = ((ILiteralNode)wrapper).Language;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Fails_invalid_DataType()
    {
        var node = _factory.CreateBlankNode();
        var wrapper = new MockWrapperNode(node);

        Assert.Throws<InvalidCastException>(() =>
            ((ILiteralNode)wrapper).DataType);
    }

    [Fact]
    public void Delegates_DataType()
    {
        var expected = UriFactory.Root.Create("urn:s");
        var node = _factory.CreateLiteralNode(string.Empty, expected);
        var wrapper = new MockWrapperNode(node);

        var actual = ((ILiteralNode)wrapper).DataType;

        Assert.Equal(expected, actual);
    }

}