/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2025 dotNetRDF Project (http://dotnetrdf.org/)
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
using System.Text;
using System.Xml.Serialization;
using VDS.RDF.Parsing;

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for Literal Nodes.
/// </summary>
[Serializable,XmlRoot(ElementName="literal")]
public abstract class BaseLiteralNode 
    : BaseNode, ILiteralNode, IEquatable<BaseLiteralNode>, IComparable<BaseLiteralNode>
{
    private readonly int _hashCode;

    /// <summary>
    /// Internal Only Constructor for Literal Nodes.
    /// </summary>
    /// <param name="literal">String value of the Literal.</param>
    /// <param name="normalize">Whether to Normalize the Literal Value.</param>
    protected internal BaseLiteralNode(string literal, bool normalize)
        : base(NodeType.Literal)
    {
        Value = normalize ? literal.Normalize() : literal;
        DataType = UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Internal Only Constructor for Literal Nodes.
    /// </summary>
    /// <param name="literal">String value of the Literal.</param>
    /// <param name="langspec">String value for the Language Specifier for the Literal.</param>
    /// <param name="normalize">Whether to Normalize the Literal Value.</param>
    protected internal BaseLiteralNode(string literal, string langspec, bool normalize)
        : base(NodeType.Literal)
    {
        Value = normalize ? literal.Normalize() : literal;
        Language = langspec != null ? langspec.ToLowerInvariant() : string.Empty;

        // Compute Hash Code
        if (Language.Equals(string.Empty))
        {
            // Empty Language Specifier equivalent to a string literal
            DataType = UriFactory.Root.Create(XmlSpecsHelper.XmlSchemaDataTypeString);
            _hashCode = ComputeHashCode();
        }
        else
        {
            DataType = UriFactory.Root.Create(RdfSpecsHelper.RdfLangString);
            _hashCode = ComputeHashCode();
        }
    }

    /// <summary>
    /// Internal Only Constructor for Literal Nodes.
    /// </summary>
    /// <param name="literal">String value of the Literal.</param>
    /// <param name="datatype">Uri for the Literals Data Type.</param>
    /// <param name="normalize">Whether to Normalize the Literal Value.</param>
    protected internal BaseLiteralNode(string literal, Uri datatype, bool normalize)
        : base(NodeType.Literal)
    {
        Value = normalize ? literal.Normalize() : literal;
        DataType = datatype;

        // Compute Hash Code
        _hashCode = ComputeHashCode();
    }

    /// <summary>
    /// Gives the String Value of the Literal.
    /// </summary>
    public string Value { get; private set; }

    /// <summary>
    /// Gives the Language Specifier for the Literal (if it exists) or the Empty String.
    /// </summary>
    public string Language { get; private set; } = string.Empty;

    /// <summary>
    /// Gives the Data Type Uri for the Literal (if it exists) or a null.
    /// </summary>
    public Uri DataType { get; private set; }

    /// <summary>
    /// Implementation of the Equals method for Literal Nodes.
    /// </summary>
    /// <param name="obj">Object to compare the Node with.</param>
    /// <returns></returns>
    /// <remarks>
    /// The default behaviour is for Literal Nodes to be considered equal IFF
    /// <ol>
    /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
    /// <li>Their Data Types are identical (or neither has a Data Type)</li>
    /// <li>Their String values are identical</li>
    /// </ol>
    /// This behaviour can be overridden to use value equality by setting the <see cref="EqualityHelper.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
    /// </remarks>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        if (ReferenceEquals(this, obj)) return true;

        return obj is INode node && Equals(node);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return _hashCode;
    }

    private int ComputeHashCode()
    {
        return (Value, DataType, Language).GetHashCode();
    }

    /// <summary>
    /// Implementation of the Equals method for Literal Nodes.
    /// </summary>
    /// <param name="other">Object to compare the Node with.</param>
    /// <returns></returns>
    /// <remarks>
    /// The default behaviour is for Literal Nodes to be considered equal IFF
    /// <ol>
    /// <li>Their Language Specifiers are identical (or neither has a Language Specifier)</li>
    /// <li>Their Data Types are identical (or neither has a Data Type)</li>
    /// <li>Their String values are identical</li>
    /// </ol>
    /// This behaviour can be overridden to use value equality by setting the <see cref="EqualityHelper.LiteralEqualityMode">LiteralEqualityMode</see> option to be <see cref="LiteralEqualityMode.Loose">Loose</see> if this is more suited to your application.
    /// </remarks>
    public override bool Equals(INode other)
    {
        if (other == null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (other.NodeType == NodeType.Literal)
        {
            return Equals((ILiteralNode)other);
        }

        // Can only be equal to a LiteralNode
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Ref Node (should always be false).
    /// </summary>
    /// <param name="other">Ref Node.</param>
    /// <returns></returns>
    public override bool Equals(IRefNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Blank Node (should always be false).
    /// </summary>
    /// <param name="other">Blank Node.</param>
    /// <returns></returns>
    public override bool Equals(IBlankNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Graph Literal Node (should always be false).
    /// </summary>
    /// <param name="other">Graph Literal Node.</param>
    /// <returns></returns>
    public override bool Equals(IGraphLiteralNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Literal Node.
    /// </summary>
    /// <param name="other">Literal Node.</param>
    /// <returns></returns>
    public override bool Equals(ILiteralNode other)
    {
        return other != null && EqualityHelper.AreLiteralsEqual(this, other);
    }

    /// <summary>
    /// Determines whether this Node is equal to a URI Node (should always be false).
    /// </summary>
    /// <param name="other">URI Node.</param>
    /// <returns></returns>
    public override bool Equals(IUriNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Variable Node (should always be false).
    /// </summary>
    /// <param name="other">Variable Node.</param>
    /// <returns></returns>
    public override bool Equals(IVariableNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Triple Node (should always be false).
    /// </summary>
    /// <param name="other">Triple Node.</param>
    /// <returns></returns>
    public override bool Equals(ITripleNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Literal Node.
    /// </summary>
    /// <param name="other">Literal Node.</param>
    /// <returns></returns>
    public bool Equals(BaseLiteralNode other)
    {
        return Equals((ILiteralNode)other);
    }

    /// <summary>
    /// Gets a String representation of a Literal Node.
    /// </summary>
    /// <returns></returns>
    /// <remarks>Gives a value without quotes (as some syntaxes use) with the Data Type/Language Specifier appended using Notation 3 syntax.</remarks>
    public override string ToString()
    {
        var stringOut = new StringBuilder();
        stringOut.Append(Value);
        if (!Language.Equals(string.Empty))
        {
            stringOut.Append("@");
            stringOut.Append(Language);
        }
        else if (!(DataType == null))
        {
            stringOut.Append("^^");
            stringOut.Append(DataType.AbsoluteUri);
        }

        return stringOut.ToString();
    }

    /// <summary>
    /// Implementation of CompareTo for Literal Nodes.
    /// </summary>
    /// <param name="other">Node to Compare To.</param>
    /// <returns></returns>
    /// <remarks>
    /// <para>Literal Nodes are greater than Blank Nodes, Uri Nodes and Nulls, they are less than Graph Literal Nodes.</para>
    /// <para>
    /// Two Literal Nodes are initially compared based upon Data Type, untyped literals are less than typed literals.
    /// Two untyped literals are compared purely on lexical value, Language Specifier has no effect on the ordering.
    /// This means Literal Nodes are only partially ordered, for example "hello"@en and "hello"@en-us are considered to be the same for ordering purposes though they are different for equality purposes.
    /// Data-typed Literals can only be properly ordered if they are one of a small subset of types (Integers, Booleans, Date Times, Strings and URIs).
    /// If the datatypes for two Literals are non-matching they are ordered on Datatype Uri, this ensures that each range of Literal Nodes is sorted to some degree.
    /// Again this also means that Literals are partially ordered since unknown datatypes will only be sorted based on lexical value and not on actual value.
    /// </para>
    /// </remarks>
    public override int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;

        return other?.NodeType switch
        {
            // Everything is greater than a null
            null => 1,
            // Literal Nodes are greater than Blank, Variable and Uri Nodes
            NodeType.Blank or NodeType.Variable or NodeType.Uri => 1,
            NodeType.Literal => CompareTo((ILiteralNode)other),
            // Anything else is considered greater than a Literal Node
            _ => -1,
        };
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IRefNode other)
    {
        // We are always greater than nulls, blank nodes or URI nodes, but less than triple nodes.
        return other?.NodeType == NodeType.Triple ? -1 : 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IBlankNode other)
    {
        // We are always greater than nulls/Blank Nodes
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ILiteralNode other)
    {
        return ReferenceEquals(this, other) ? 0 : ComparisonHelper.CompareLiterals(this, other);
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IGraphLiteralNode other)
    {
        // We are greater than nulls but less than graph literals
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IUriNode other)
    {
        // We are always greater than nulls/URI Nodes
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IVariableNode other)
    {
        // We are always greater than nulls/Variable Nodes
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ITripleNode other)
    {
        // We are greater than nulls, but less than triple nodes
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public int CompareTo(BaseLiteralNode other)
    {
        return CompareTo((ILiteralNode)other);
    }


}