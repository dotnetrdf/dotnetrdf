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
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Storage.Virtualisation;

/// <summary>
/// Abstract Base implementation of a Virtual Literal Node.
/// </summary>
/// <typeparam name="TNodeID">Node ID Type.</typeparam>
/// <typeparam name="TGraphID">Graph ID Type.</typeparam>
public abstract class BaseVirtualLiteralNode<TNodeID, TGraphID>
    : BaseVirtualNode<TNodeID, TGraphID>, ILiteralNode,
        IEquatable<BaseVirtualLiteralNode<TNodeID, TGraphID>>, IComparable<BaseVirtualLiteralNode<TNodeID, TGraphID>>,
        IValuedNode
{
    private string _litValue, _lang;
    private Uri _datatype;
    private IValuedNode _strongValue;

    /// <summary>
    /// Creates a new Virtual Literal Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    protected BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider)
        : base(g, NodeType.Literal, id, provider) { }

    /// <summary>
    /// Creates a new Virtual Literal Node.
    /// </summary>
    /// <param name="g">Graph the Node belongs to.</param>
    /// <param name="id">Virtual ID.</param>
    /// <param name="provider">Virtual RDF Provider.</param>
    /// <param name="value">Materialised Value.</param>
    protected BaseVirtualLiteralNode(IGraph g, TNodeID id, IVirtualRdfProvider<TNodeID, TGraphID> provider, ILiteralNode value)
        : base(g, NodeType.Literal, id, provider, value) { }

    /// <summary>
    /// Takes post materialisation actions.
    /// </summary>
    protected sealed override void OnMaterialise()
    {
        var temp = (ILiteralNode)_value;
        _litValue = temp.Value;
        _lang = temp.Language;
        _datatype = temp.DataType;
    }

    /// <summary>
    /// Gets the lexical value of the Literal.
    /// </summary>
    public string Value
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _litValue;
        }
    }

    /// <summary>
    /// Gets the language specifier (if any) of the Literal.
    /// </summary>
    public string Language
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _lang;
        }
    }

    /// <summary>
    /// Gets the Datatype (if any) of the Literal.
    /// </summary>
    public Uri DataType
    {
        get
        {
            if (_value == null) MaterialiseValue();
            return _datatype;
        }
    }

    /// <summary>
    /// Compares this Node to another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public override int CompareTo(ILiteralNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        if (other == null) return 1;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual) && areEqual) return 0;

        return ComparisonHelper.CompareLiterals(this, other);

    }

    /// <summary>
    /// Checks this Node for equality against another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public override bool Equals(ILiteralNode other)
    {
        if (ReferenceEquals(this, other)) return true;
        if (other == null) return false;
        bool areEqual;
        if (TryVirtualEquality(other, out areEqual)) return areEqual;

        return EqualityHelper.AreLiteralsEqual(this, other);
    }

    /// <summary>
    /// Checks this Node for equality against another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform the equality check.
    /// </remarks>
    public bool Equals(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
    {
        return Equals((ILiteralNode)other);
    }

    /// <summary>
    /// Compares this Node to another Literal Node.
    /// </summary>
    /// <param name="other">Other Literal Node.</param>
    /// <returns></returns>
    /// <remarks>
    /// Unless Virtual Equality (equality based on the Virtual RDF Provider and Virtual ID) can be determined or the Nodes are of different types then the Nodes value will have to be materialised in order to perform comparison.
    /// </remarks>
    public int CompareTo(BaseVirtualLiteralNode<TNodeID, TGraphID> other)
    {
        return CompareTo((ILiteralNode)other);
    }

    #region IValuedNode Members

    /// <summary>
    /// Ensures that a strong value has been determined for this node.
    /// </summary>
    private void EnsureStrongValue()
    {
        if (_strongValue == null)
        {
            if (_value == null) MaterialiseValue();
            _strongValue = _value.AsValuedNode();
        }
    }

    /// <summary>
    /// Gets the value as a string.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public string AsString()
    {
        EnsureStrongValue();
        return _strongValue.AsString();
    }

    /// <summary>
    /// Gets the value as an integer.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public long AsInteger()
    {
        EnsureStrongValue();
        return _strongValue.AsInteger();
    }

    /// <summary>
    /// Gets the value as a decimal.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public decimal AsDecimal()
    {
        EnsureStrongValue();
        return _strongValue.AsDecimal();
    }

    /// <summary>
    /// Gets the value as a float.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public float AsFloat()
    {
        EnsureStrongValue();
        return _strongValue.AsFloat();
    }

    /// <summary>
    /// Gets the value as a double.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public double AsDouble()
    {
        EnsureStrongValue();
        return _strongValue.AsDouble();
    }

    /// <summary>
    /// Gets the value as a boolean.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public bool AsBoolean()
    {
        EnsureStrongValue();
        return _strongValue.AsBoolean();
    }

    /// <summary>
    /// Gets the value as a date time.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public DateTime AsDateTime()
    {
        EnsureStrongValue();
        return _strongValue.AsDateTime();
    }

    /// <summary>
    /// Gets the value as a date time.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public DateTimeOffset AsDateTimeOffset()
    {
        EnsureStrongValue();
        return _strongValue.AsDateTimeOffset();
    }

    /// <summary>
    /// Gets the value as a time span.
    /// </summary>
    /// <returns></returns>
    /// <remarks>
    /// Forces a materialisation of the value.
    /// </remarks>
    public TimeSpan AsTimeSpan()
    {
        EnsureStrongValue();
        return _strongValue.AsTimeSpan();
    }

    /// <summary>
    /// Gets the URI of the datatype this valued node represents as a String.
    /// </summary>
    public string EffectiveType
    {
        get
        {
            EnsureStrongValue();
            return _strongValue.EffectiveType;
        }
    }

    /// <summary>
    /// Gets the numeric type of the node.
    /// </summary>
    public SparqlNumericType NumericType
    {
        get 
        {
            EnsureStrongValue();
            return _strongValue.NumericType;
        }
    }

    #endregion
}