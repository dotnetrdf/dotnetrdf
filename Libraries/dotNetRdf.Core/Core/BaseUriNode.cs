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
using System.Diagnostics;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for URI Nodes.
/// </summary>
[DebuggerDisplay("{" + nameof(_uri) + "}")]
public abstract class BaseUriNode
    : BaseNode, IUriNode, IEquatable<BaseUriNode>, IComparable<BaseUriNode>, IValuedNode
{
    private Uri _uri;
    private readonly int _hashCode;

    /// <summary>
    /// Internal Only Constructor for URI Nodes.
    /// </summary>
    /// <param name="uri">URI.</param>
    protected internal BaseUriNode(Uri uri)
        : base(NodeType.Uri)
    {
        _uri = uri ?? throw new ArgumentNullException(nameof(uri));

        // Compute Hash Code
        _hashCode = (_nodeType, _uri.AbsoluteUri).GetHashCode();
    }

    /// <summary>
    /// Gets the Uri for this Node.
    /// </summary>
    public virtual Uri Uri => _uri;

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <summary>
    /// Implementation of Equality for Uri Nodes.
    /// </summary>
    /// <param name="obj">Object to compare with.</param>
    /// <returns></returns>
    /// <remarks>
    /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison.
    /// </remarks>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        if (ReferenceEquals(this, obj)) return true;

        if (obj is INode node)
        {
            return Equals(node);
        }

        // Can only be equal to other Nodes
        return false;
    }

    /// <summary>
    /// Implementation of Equality for Uri Nodes.
    /// </summary>
    /// <param name="other">Object to compare with.</param>
    /// <returns></returns>
    /// <remarks>
    /// URI Nodes are considered equal if the string form of their URIs match using Ordinal string comparison.
    /// </remarks>
    public override bool Equals(INode other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (other.NodeType == NodeType.Uri)
        {
            Uri temp = ((IUriNode)other).Uri;

            return EqualityHelper.AreUrisEqual(_uri, temp);
        }

        // Can only be equal to UriNodes
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Blank Node (should always be false).
    /// </summary>
    /// <param name="other">Blank Node.</param>
    /// <returns></returns>
    public override bool Equals(IRefNode other)
    {
        if (other is IUriNode uriNode)
        {
            return Equals(uriNode);
        }

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
    /// Determines whether this Node is equal to a Literal Node (should always be false).
    /// </summary>
    /// <param name="other">Literal Node.</param>
    /// <returns></returns>
    public override bool Equals(ILiteralNode other)
    {
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a URI Node.
    /// </summary>
    /// <param name="other">URI Node.</param>
    /// <returns></returns>
    public override bool Equals(IUriNode other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        return EqualityHelper.AreUrisEqual(_uri, other.Uri);
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
    /// Determines whether this Node is equal to a URI Node.
    /// </summary>
    /// <param name="other">URI Node.</param>
    /// <returns></returns>
    public bool Equals(BaseUriNode other)
    {
        return Equals((IUriNode)other);
    }

    /// <summary>
    /// Gets a String representation of a Uri as a plain text Uri.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return _uri.AbsoluteUri;
    }

    /// <summary>
    /// Implementation of Compare To for Uri Nodes.
    /// </summary>
    /// <param name="other">Node to Compare To.</param>
    /// <returns></returns>
    /// <remarks>
    /// Uri Nodes are greater than Blank Nodes and Nulls, they are less than Literal Nodes and Graph Literal Nodes.
    /// <br /><br />
    /// Uri Nodes are ordered based upon lexical ordering of the string value of their URIs.
    /// </remarks>
    public override int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;

        return other?.NodeType switch
        {
            // Everything is greater than a null.
            // URI Nodes are greater than blank nodes and variable nodes
            null or NodeType.Blank or NodeType.Variable => 1,
            NodeType.Uri => CompareTo((IUriNode)other),
            // Everything else is greater than a URI nodes
            _ => -1
        };
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IRefNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other is IUriNode uriNode) return CompareTo(uriNode);
        // Otherwise other is null or a blank node and we are greater than either of those
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IBlankNode other)
    {
        // URI Nodes are greater than nulls and Blank Nodes
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IGraphLiteralNode other)
    {
        // We are greater than nulls, less than graph literal nodes
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ILiteralNode other)
    {
        // We are greater than nulls, less than literal nodes
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IVariableNode other)
    {
        // URI Nodes are greater than nulls and Variable Nodes
        return 1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IUriNode other)
    {
        return ComparisonHelper.CompareUris(Uri, other.Uri);
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
    public int CompareTo(BaseUriNode other)
    {
        return CompareTo((IUriNode)other);
    }

    #region IValuedNode Members

    /// <summary>
    /// Gets the value of the node as a string.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        return _uri.AbsoluteUri;
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to a boolean.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to a date time.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to a date time.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URIs cannot be cast to a time span.
    /// </summary>
    /// <returns></returns>
    public TimeSpan AsTimeSpan()
    {
        throw new RdfQueryException("Cannot case a URI to a type");
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType
    {
        get
        {
            return string.Empty;
        }
    }

    /// <summary>
    /// Gets the numeric type of the expression.
    /// </summary>
    public SparqlNumericType NumericType
    {
        get
        {
            return SparqlNumericType.NaN;
        }
    }

    #endregion
}