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
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF;

/// <summary>
/// Abstract Base Class for Blank Nodes.
/// </summary>
public abstract class BaseBlankNode
    : BaseNode, IBlankNode, IEquatable<BaseBlankNode>, IComparable<BaseBlankNode>, IValuedNode
{
    private readonly int _hashCode;

    /// <summary>
    /// Internal Only constructor for Blank Nodes.
    /// </summary>
    /// <param name="nodeId">Custom Node ID to use.</param>
    protected internal BaseBlankNode(string nodeId)
        : base(NodeType.Blank)
    {
        if (nodeId == null)
        {
            throw new ArgumentNullException(nameof(nodeId));
        }
        if (nodeId.Equals(string.Empty))
        {
            throw new RdfException("Cannot create a Blank Node with an empty ID");
        }
        InternalID = nodeId;

        // Compute Hash Code
        _hashCode = (NodeType + InternalID).GetHashCode();
    }

    /// <summary>
    /// Internal Only constructor for Blank Nodes.
    /// </summary>
    /// <param name="factory">Node Factory from which to obtain a Node ID.</param>
    protected internal BaseBlankNode(INodeFactory factory)
        : this(factory.GetNextBlankNodeID())
    {
        HasAutoAssignedID = true;
    }

    /// <summary>
    /// Returns the Internal Blank Node ID this Node has in the Graph.
    /// </summary>
    /// <remarks>
    /// Usually automatically assigned and of the form autosXXX where XXX is some number.  If an RDF document contains a Blank Node ID of this form that clashes with an existing auto-assigned ID it will be automatically remapped by the Graph using the <see cref="BlankNodeMapper">BlankNodeMapper</see> when it is created.
    /// </remarks>
    public string InternalID { get; }

    /// <summary>
    /// Indicates whether this Blank Node had its ID assigned for it by the Graph.
    /// </summary>
    public bool HasAutoAssignedID { get; }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <summary>
    /// Implementation of Equals for Blank Nodes.
    /// </summary>
    /// <param name="obj">Object to compare with the Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Blank Nodes are considered equal if their internal IDs match precisely and they originate from the same Graph.
    /// </remarks>
    public override bool Equals(object obj)
    {
        if (obj == null) return false;

        if (ReferenceEquals(this, obj)) return true;

        if (obj is INode node)
        {
            return Equals(node);
        }

        // Can only be equal to things which are Nodes
        return false;
    }

    /// <summary>
    /// Implementation of Equals for Blank Nodes.
    /// </summary>
    /// <param name="other">Object to compare with the Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Blank Nodes are considered equal if their internal IDs match precisely and they originate from the same Graph.
    /// </remarks>
    public override bool Equals(INode other)
    {
        if (other == null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (other.NodeType == NodeType.Blank)
        {
            return EqualityHelper.AreBlankNodesEqual(this, (IBlankNode)other);
        }

        // Can only be equal to Blank Nodes
        return false;
    }

    /// <summary>
    /// Determines whether this node is equal to another IRefNode.
    /// </summary>
    /// <param name="other">Other node.</param>
    /// <returns></returns>
    public override bool Equals(IRefNode other)
    {
        if (other == null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (other.NodeType == NodeType.Blank)
        {
            return EqualityHelper.AreBlankNodesEqual(this, (IBlankNode)other);
        }
        // Can only be equal to blank nodes
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to another.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    public override bool Equals(IBlankNode other)
    {
        return EqualityHelper.AreBlankNodesEqual(this, other);
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
    /// Determines whether this Node is equal to a Blank Node.
    /// </summary>
    /// <param name="other">Blank Node.</param>
    /// <returns></returns>
    public bool Equals(BaseBlankNode other)
    {
        return Equals((IBlankNode)other);
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other?.NodeType switch
        {
            // We are greater than nulls and variables
            null or NodeType.Variable => 1,
            NodeType.Blank => ComparisonHelper.CompareBlankNodes(this, other as IBlankNode),
            // We are less than all other types of node
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
        return other?.NodeType switch
        {
            // We are greater than nulls and variables
            null or NodeType.Variable => 1,
            NodeType.Blank => ComparisonHelper.CompareBlankNodes(this, other as IBlankNode),
            // We are less than all other types of node
            _ => -1
        };
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IBlankNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other == null ? 1 : ComparisonHelper.CompareBlankNodes(this, other);
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
    public override int CompareTo(IUriNode other)
    {
        // We are greater than nulls, less than uri nodes
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IVariableNode other)
    {
        // We are always greater than Nulls and Variable Nodes
        return 1;
    }

    /// <summary>
    /// Returns an integer indicating the ordering of this Node compared to a Triple Node.
    /// </summary>
    /// <param name="other">Triple Node to compare with.</param>
    /// <returns></returns>
    public override int CompareTo(ITripleNode other)
    {
        // We are always greater than Nulls, and less than Triple Nodes
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public int CompareTo(BaseBlankNode other)
    {
        return CompareTo((IBlankNode)other);
    }

    /// <summary>
    /// Returns a string representation of this Blank Node in QName form.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "_:" + InternalID;
    }

    #region IValuedNode Members

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a String.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to an integer.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a decimal.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a float.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a double.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a boolean.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a date time.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be cast to a date time offset.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Throws an error as a Blank Node cannot be case to a time span.
    /// </summary>
    /// <returns></returns>
    public TimeSpan AsTimeSpan()
    {
        throw new RdfQueryException("Unable to cast a Blank Node to a type");
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType => string.Empty;

    /// <summary>
    /// Gets the Numeric Type of the Node.
    /// </summary>
    public SparqlNumericType NumericType => SparqlNumericType.NaN;

    #endregion
}