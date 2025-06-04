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
/// Abstract Base implementation of a Virtual URI Node.
/// </summary>
/// <typeparam name="TNodeID">Node ID Type.</typeparam>
/// <typeparam name="TGraphID">Graph ID Type.</typeparam>
public abstract class BaseVirtualUriNode<TNodeID, TGraphID>
    : BaseVirtualNode<TNodeID, TGraphID>, IUriNode,
        IEquatable<BaseVirtualUriNode<TNodeID, TGraphID>>, IComparable<BaseVirtualUriNode<TNodeID, TGraphID>>,
        IValuedNode
{
    private Uri _u;

    /// <summary>
    /// Creates a new Virtual URI Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    protected BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        : base(g, NodeType.Uri, id, provider) { }

    /// <summary>
    /// Creates a new Virtual URI Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    /// <param name="value">Materialised Value.</param>
    protected BaseVirtualUriNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IUriNode value)
        : base(g, NodeType.Uri, id, provider, value) { }

    /// <summary>
    /// Takes post materialisation actions.
    /// </summary>
    protected sealed override void OnMaterialise()
    {
        var temp = (IUriNode)_value;
        _u = temp.Uri;
    }

    /// <summary>
    /// Gets the URI.
    /// </summary>
    public Uri Uri
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _u;
        }
    }

    /// <summary>
    /// Compares this Node to another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public override int CompareTo(IUriNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual) return 0;

        return ComparisonHelper.CompareUris(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public override bool Equals(IUriNode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual)) return areEqual;

        return EqualityHelper.AreUrisEqual(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public bool Equals(BaseVirtualUriNode<TNodeID, TGraphID> other)
    {
        return Equals((IUriNode)other);
    }

    /// <summary>
    /// Compares this Node to another URI Node.
    /// </summary>
    /// <param name="other">Other URI Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public int CompareTo(BaseVirtualUriNode<TNodeID, TGraphID> other)
    {
        return CompareTo((IUriNode)other);
    }

    #region IValuedNode Members

    /// <summary>
    /// Gets the string value of the node.
    /// </summary>
    public string AsString()
    {
        return Uri.ToString();
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to numerics.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to a boolean.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to a date time.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Throws an error as URI nodes cannot be cast to a date time.
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
        throw new RdfQueryException("Cannot cast a URI to a type");
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType => string.Empty;

    /// <summary>
    /// Gets the numeric type of the expression.
    /// </summary>
    public SparqlNumericType NumericType => SparqlNumericType.NaN;

    #endregion
}