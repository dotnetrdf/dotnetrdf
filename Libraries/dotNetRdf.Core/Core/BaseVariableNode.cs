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
/// Abstract Base Class for Variable Nodes.
/// </summary>
public abstract class BaseVariableNode
    : BaseNode, IVariableNode, IEquatable<BaseVariableNode>, IComparable<BaseVariableNode>, IValuedNode
{
    private readonly int _hashCode;

    /// <summary>
    /// Creates a new Variable Node.
    /// </summary>
    /// <param name="varName">Variable Name.</param>
    protected internal BaseVariableNode(string varName)
        : base(NodeType.Variable)
    {
        if (varName.StartsWith("?") || varName.StartsWith("$"))
        {
            VariableName = varName.Substring(1);
        }
        else
        {
            VariableName = varName;
        }
        _hashCode = (_nodeType + varName).GetHashCode();
    }

    /// <summary>
    /// Gets the Variable Name.
    /// </summary>
    public string VariableName { get; }

    /// <summary>Serves as the default hash function.</summary>
    /// <returns>A hash code for the current object.</returns>
    public override int GetHashCode()
    {
        return _hashCode;
    }

    /// <summary>
    /// Gets whether this Node is equal to some other Node.
    /// </summary>
    /// <param name="other">Node to test.</param>
    /// <returns></returns>
    public override bool Equals(INode other)
    {
        if (other is null) return false;

        if (ReferenceEquals(this, other)) return true;

        if (other is IVariableNode varNode)
        {
            return EqualityHelper.AreVariablesEqual(this, varNode);
        }

        // Can only be equal to other Variables
        return false;
    }

    /// <summary>
    /// Determines whether this Node is equal to a Blank Node (should always be false).
    /// </summary>
    /// <param name="other">Blank Node.</param>
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
    /// Determines whether this Node is equal to a Variable Node.
    /// </summary>
    /// <param name="other">Variable Node.</param>
    /// <returns></returns>
    public override bool Equals(IVariableNode other)
    {
        if (other is null) return false;

        return ReferenceEquals(this, other) || EqualityHelper.AreVariablesEqual(this, other);
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
    /// Determines whether this Node is equal to a Variable Node.
    /// </summary>
    /// <param name="other">Variable Node.</param>
    /// <returns></returns>
    public bool Equals(BaseVariableNode other)
    {
        return Equals((IVariableNode)other);
    }

    /// <summary>
    /// Gets whether this Node is equal to some Object.
    /// </summary>
    /// <param name="obj">Object to test.</param>
    /// <returns></returns>
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
    /// Gets the String representation of this Node.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "?" + VariableName;
    }

    /// <summary>
    /// Compares this Node to another Node.
    /// </summary>
    /// <param name="other">Node to compare with.</param>
    /// <returns></returns>
    public override int CompareTo(INode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other?.NodeType switch
        {
            // Variables are considered greater than null
            null => 1,
            NodeType.Variable => CompareTo(other as IVariableNode),
            // Variable Nodes are less than everything else
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
        // Variables are considered greater than null and less than everything else
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IBlankNode other)
    {
        // Variables are considered greater than null and less than everything else
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IGraphLiteralNode other)
    {
        // Variables are considered greater than null and less than everything else
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ILiteralNode other)
    {
        // Variables are considered greater than null and less than everything else
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IUriNode other)
    {
        // Variables are considered greater than null and less than everything else
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(IVariableNode other)
    {
        if (ReferenceEquals(this, other)) return 0;
        return other == null ? 1 : ComparisonHelper.CompareVariables(this, other);
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public override int CompareTo(ITripleNode other)
    {
        // We are always greater than null, and less than a Triple node.
        return other == null ? 1 : -1;
    }

    /// <summary>
    /// Returns an Integer indicating the Ordering of this Node compared to another Node.
    /// </summary>
    /// <param name="other">Node to test against.</param>
    /// <returns></returns>
    public int CompareTo(BaseVariableNode other)
    {
        return CompareTo((IVariableNode)other);
    }


    #region IValuedNode Members

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public string AsString()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public long AsInteger()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public decimal AsDecimal()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public float AsFloat()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public double AsDouble()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public bool AsBoolean()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
    /// </summary>
    /// <returns></returns>
    public DateTime AsDateTime()
    {
        throw new RdfQueryException("Cannot cast Variable Nodes to types");
    }

    /// <summary>
    /// Throws an error as variables cannot be converted to types.
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
        throw new RdfQueryException("Cannot cast Variable Nodes to a types");
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