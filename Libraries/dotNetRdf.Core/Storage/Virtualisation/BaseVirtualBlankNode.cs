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

namespace VDS.RDF.Storage.Virtualisation;

/// <summary>
/// Abstract Base implementation of a Virtual Blank Node.
/// </summary>
/// <typeparam name="TNodeID">Node ID Type.</typeparam>
/// <typeparam name="TGraphID">Graph ID Type.</typeparam>
public abstract class BaseVirtualBlankNode<TNodeID, TGraphID>
    : BaseVirtualNode<TNodeID, TGraphID>, IBlankNode, 
        IEquatable<BaseVirtualBlankNode<TNodeID, TGraphID>>, IComparable<BaseVirtualBlankNode<TNodeID, TGraphID>>,
        IValuedNode
{
    private string _internalID;

    /// <summary>
    /// Creates a new Virtual Blank Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    protected BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        : base(g, NodeType.Blank, id, provider) { }

    /// <summary>
    /// Creates a new Virtual Blank Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    /// <param name="value">Materialised Value.</param>
    protected BaseVirtualBlankNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IBlankNode value)
        : base(g, NodeType.Blank, id, provider, value) { }

    /// <summary>
    /// Takes post materialisation actions.
    /// </summary>
    protected sealed override void OnMaterialise()
    {
        var temp = (IBlankNode)_value;
        _internalID = temp.InternalID;
    }

    /// <summary>
    /// Gets the Internal ID of the Blank Node.
    /// </summary>
    public string InternalID
    {
        get 
        {
            if (_value == null) MaterialiseValue();
            return _internalID;
        }
    }

    /// <summary>
    /// Compares this Node to another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public override int CompareTo(IBlankNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual) return 0;

        return ComparisonHelper.CompareBlankNodes(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public override bool Equals(IBlankNode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual)) return areEqual;

        return EqualityHelper.AreBlankNodesEqual(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public bool Equals(BaseVirtualBlankNode<TNodeID, TGraphID> other)
    {
        return Equals((IBlankNode)other);
    }

    /// <summary>
    /// Compares this Node to another Blank Node.
    /// </summary>
    /// <param name="other">Other Blank Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public int CompareTo(BaseVirtualBlankNode<TNodeID, TGraphID> other)
    {
        return CompareTo((IBlankNode)other);
    }

    #region IValuedNode Members

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
    }

    /// <summary>
    /// Throws an error as blank nodes cannot be cast to a time span.
    /// </summary>
    /// <returns></returns>
    public TimeSpan AsTimeSpan()
    {
        throw new RdfQueryException("Cannot cast Blank Nodes to types");
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
    /// Gets the numeric type of the node.
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