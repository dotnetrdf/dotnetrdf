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
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF;

/// <summary>
/// Base class for triple nodes.
/// </summary>
public abstract class BaseTripleNode 
    : BaseNode, ITripleNode, IEquatable<BaseTripleNode>, IComparable<BaseTripleNode>, IValuedNode
{
    /// <inheritdoc />
    public Triple Triple { get; }

    /// <summary>
    /// Internal-only constructor for triple nodes.
    /// </summary>
    /// <param name="triple">The triple that is the value of this node.</param>
    protected internal BaseTripleNode(Triple triple) : base(NodeType.Triple)
    {
        Triple = triple;
    }

    /// <summary>
    /// Object equality for triple nodes.
    /// </summary>
    /// <param name="obj">The object to compare this node with.</param>
    /// <returns>True if <paramref name="obj"/> is an Triple node and its triple value compares as equal to this node's triple value.</returns>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;
        if (ReferenceEquals(this, obj)) return true;
        return obj is INode node && Equals(node);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return Triple.GetHashCode();
    }

    /// <summary>
    /// Determines whether this node is equal to another node.
    /// </summary>
    /// <param name="other">The other node to compare with.</param>
    /// <returns>True if <paramref name="other"/> is a triple node whose triple compares as equal to the triple of this node, false otherwise.</returns>
    public override bool Equals(INode other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return other.NodeType == NodeType.Triple && Equals(other as ITripleNode);
    }

    /// <summary>
    /// Determines whether this node is equal to another resource reference node.
    /// </summary>
    /// <param name="other">The other node to compare with.</param>
    /// <returns></returns>
    public override bool Equals(IRefNode other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        return other.NodeType == NodeType.Triple && Equals(other as ITripleNode);
    }

    /// <summary>
    /// Determines whether this node is equal to a blank node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <returns>Always false.</returns>
    public override bool Equals(IBlankNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to a graph literal node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <returns>Always false.</returns>
    public override bool Equals(IGraphLiteralNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to a literal node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <returns>Always false.</returns>
    public override bool Equals(ILiteralNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to a URI node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <returns>Always false.</returns>
    public override bool Equals(IUriNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to a variable node.
    /// </summary>
    /// <param name="other">The node to compare to.</param>
    /// <returns>Always false.</returns>
    public override bool Equals(IVariableNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to another triple node.
    /// </summary>
    /// <param name="other">The other node to compare with.</param>
    /// <returns>True if the <see cref="Triple"/> property of <paramref name="other"/> compares as equal to the <see cref="Triple"/> property of this node, false otherwise.</returns>
    public override bool Equals(ITripleNode other)
    {
        return other != null && other.Triple.Equals(Triple);
    }

    /// <summary>
    /// Determines whether this node is equal to another triple node.
    /// </summary>
    /// <param name="other">The other node to compare with.</param>
    /// <returns>True if the <see cref="Triple"/> property of <paramref name="other"/> compares as equal to the <see cref="Triple"/> property of this node, false otherwise.</returns>
    public bool Equals(BaseTripleNode other)
    {
        return Equals(other as ITripleNode);
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        // Greater than anything other than another triple node
        return other?.NodeType == NodeType.Triple ? CompareTo(other as ITripleNode) : 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IRefNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other?.NodeType == NodeType.Triple ? CompareTo(other as ITripleNode) : 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IBlankNode other)
    {
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IGraphLiteralNode other)
    {
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ILiteralNode other)
    {
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IUriNode other)
    {
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IVariableNode other)
    {
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ITripleNode other)
    {
        return other == null ? 1 : Triple.CompareTo(other.Triple);
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public int CompareTo(BaseTripleNode other)
    {
        return CompareTo(other as ITripleNode);
    }

    /// <summary>
    /// Get a string representation of a triple node.
    /// </summary>
    /// <returns>Gives the string value of <see cref="Triple"/> enclosed in the RDF-star &lt;&lt; &gt;&gt; quotes.</returns>
    public override string ToString()
    {
        var stringOut = new StringBuilder();
        stringOut.Append("<<");
        stringOut.Append(Triple);
        stringOut.Append(">>");
        return stringOut.ToString();
    }

    /// <inheritdoc />
    public string AsString()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public long AsInteger()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public float AsFloat()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public double AsDouble()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public bool AsBoolean()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public TimeSpan AsTimeSpan()
    {
        throw new RdfQueryException("Unable to cast a Triple Node to a type");
    }

    /// <inheritdoc />
    public string EffectiveType => string.Empty;

    /// <inheritdoc />
    public SparqlNumericType NumericType => SparqlNumericType.NaN;
}
