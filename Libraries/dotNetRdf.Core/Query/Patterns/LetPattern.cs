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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Patterns;

/// <summary>
/// Class for representing LET Patterns in SPARQL Queries.
/// </summary>
public class LetPattern
    : BaseTriplePattern, IComparable<LetPattern>, IAssignmentPattern
{
    /// <summary>
    /// Creates a new LET Pattern.
    /// </summary>
    /// <param name="var">Variable to assign to.</param>
    /// <param name="expr">Expression which generates a value which will be assigned to the variable.</param>
    public LetPattern(string var, ISparqlExpression expr)
    {
        VariableName = var;
        AssignExpression = expr;
        _vars = VariableName.AsEnumerable().Concat(AssignExpression.Variables).Distinct().ToList();
        _vars.Sort();
    }

    /// <summary>
    /// Gets the Pattern Type.
    /// </summary>
    public override TriplePatternType PatternType
    {
        get
        {
            return TriplePatternType.LetAssignment;
        }
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessLetPattern(this, context);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitLetPattern(this);
    }

    /// <summary>
    /// Returns that this is not an accept all since it is a LET assignment.
    /// </summary>
    public override bool IsAcceptAll
    {
        get 
        {
            return false; 
        }
    }

    /// <summary>
    /// Gets the Expression that is used to generate values to be assigned.
    /// </summary>
    public ISparqlExpression AssignExpression { get; }

    /// <summary>
    /// Gets the Name of the Variable to which values will be assigned.
    /// </summary>
    public string VariableName { get; }

    /// <summary>
    /// Returns an empty enumeration as any evaluation error will result in an unbound value so we can't guarantee any variables are bound.
    /// </summary>
    public override IEnumerable<string> FixedVariables
    {
        get { return Enumerable.Empty<string>(); }
    }

    /// <summary>
    /// Returns the variable being assigned to as any evaluation error will result in an unbound value so we can't guarantee it is bound.
    /// </summary>
    public override IEnumerable<string> FloatingVariables
    {
        get { return VariableName.AsEnumerable(); }
    }

    /// <summary>
    /// Gets whether the Pattern uses the Default Dataset.
    /// </summary>
    public override bool UsesDefaultDataset
    {
        get
        {
            return AssignExpression.UsesDefaultDataset();
        }
    }

    /// <summary>
    /// Returns true as a LET can never contain Blank Nodes.
    /// </summary>
    public override bool HasNoBlankVariables
    {
        get 
        {
            return true;
        }
    }

    /// <summary>
    /// Gets the string representation of the LET assignment.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("LET(");
        output.Append("?");
        output.Append(VariableName);
        output.Append(" := ");
        output.Append(AssignExpression);
        output.Append(")");

        return output.ToString();
    }

    /// <summary>
    /// Compares this Let to another Let.
    /// </summary>
    /// <param name="other">Let to compare to.</param>
    /// <returns>Just calls the base compare method since that implements all the logic we need.</returns>
    public int CompareTo(LetPattern other)
    {
        return base.CompareTo(other);
    }

    /// <summary>
    /// Compares this Let to another Let.
    /// </summary>
    /// <param name="other">Let to compare to.</param>
    /// <returns>Just calls the base compare method since that implements all the logic we need.</returns>
    public int CompareTo(IAssignmentPattern other)
    {
        return base.CompareTo(other);
    }
}
