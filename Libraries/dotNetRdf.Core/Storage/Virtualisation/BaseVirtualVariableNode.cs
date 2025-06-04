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
/// Abstract Base implementation of a Virtual Variable Node.
/// </summary>
/// <typeparam name="TNodeID">Node ID Type.</typeparam>
/// <typeparam name="TGraphID">Graph ID Type.</typeparam>
public abstract class BaseVirtualVariableNode<TNodeID, TGraphID>
    : BaseVirtualNode<TNodeID, TGraphID>, IVariableNode,
      IEquatable<BaseVirtualVariableNode<TNodeID, TGraphID>>, IComparable<BaseVirtualVariableNode<TNodeID, TGraphID>>,
      IValuedNode
{
    private string _var;

    /// <summary>
    /// Creates a new Virtual Variable Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    protected BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        : base(g, NodeType.Variable, id, provider) { }

    /// <summary>
    /// Creates a new Virtual Variable Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    /// <param name="value">Materialised Value.</param>
    protected BaseVirtualVariableNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, IVariableNode value)
        : base(g, NodeType.Variable, id, provider, value) { }

    /// <summary>
    /// Takes post materialisation actions.
    /// </summary>
    protected sealed override void OnMaterialise()
    {
        var temp = (IVariableNode)_value;
        _var = temp.VariableName;
    }

    /// <summary>
    /// Gets the Variable Name.
    /// </summary>
    public string VariableName
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _var;
        }
    }

    /// <summary>
    /// Compares this Node to another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public override int CompareTo(IVariableNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual) return 0;

        return ComparisonHelper.CompareVariables(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public override bool Equals(IVariableNode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual)) return areEqual;

        return EqualityHelper.AreVariablesEqual(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public bool Equals(BaseVirtualVariableNode<TNodeID, TGraphID> other)
    {
        return Equals((IVariableNode)other);
    }

    /// <summary>
    /// Compares this Node to another Variable Node.
    /// </summary>
    /// <param name="other">Other Variable Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public int CompareTo(BaseVirtualVariableNode<TNodeID, TGraphID> other)
    {
        return CompareTo((IVariableNode)other);
    }

    #region IValuedNode Members

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables nodes cannot be cast to types.
    /// </summary>
    /// <returns></returns>
    public DateTimeOffset AsDateTimeOffset()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be cast to a time span.
    /// </summary>
    /// <returns></returns>
    public TimeSpan AsTimeSpan()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType => string.Empty;

    /// <summary>
    /// Gets the numeric type of the node.
    /// </summary>
    public SparqlNumericType NumericType => SparqlNumericType.NaN;

    #endregion
}
