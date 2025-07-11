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
using VDS.RDF.Query.Optimisation;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents the Selection step of Query Evaluation.
/// </summary>
/// <remarks>
/// Selection trims variables from the Multiset that are not needed in the final output.
/// </remarks>
public class Select
    : IUnaryOperator
{
    private readonly ISparqlAlgebra _pattern;
    private readonly List<SparqlVariable> _variables = new List<SparqlVariable>();

    /// <summary>
    /// Creates a new Select.
    /// </summary>
    /// <param name="pattern">Inner Pattern.</param>
    /// <param name="selectAll">Whether we are selecting all variables.</param>
    /// <param name="variables">Variables to Select.</param>
    public Select(ISparqlAlgebra pattern, bool selectAll, IEnumerable<SparqlVariable> variables)
    {
        _pattern = pattern;
        _variables.AddRange(variables);
        IsSelectAll = selectAll;
    }

    /// <summary>
    /// Does this operator select all variables?.
    /// </summary>
    public bool IsSelectAll { get; }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra
    {
        get
        {
            return _pattern;
        }
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return _pattern.Variables.Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // For a SELECT * all floating variables from inner algebra are floating
            // For SELECT ?vars all floating variables that are selected are floating
            return IsSelectAll ? _pattern.FloatingVariables : _pattern.FloatingVariables.Where(v => _variables.Any(rv => rv.Name.Equals(v) && rv.IsResultVariable));
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get
        {
            // For a SELECT * all fixed variables from inner algebra are fixed
            // For SELECT ?vars all fixed variables that are selected are fixed
            return IsSelectAll ? _pattern.FixedVariables : _pattern.FixedVariables.Where(v => _variables.Any(rv => rv.Name.Equals(v) && rv.IsResultVariable));
        }
    }

    /// <summary>
    /// Gets the SPARQL Variables used.
    /// </summary>
    /// <remarks>
    /// If the Query supplied in the <see cref="SparqlEvaluationContext">SparqlEvaluationContext</see> is non-null then it's Variables are used rather than these.
    /// </remarks>
    public IEnumerable<SparqlVariable> SparqlVariables
    {
        get
        {
            return _variables;
        }
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Select(" + _pattern + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessSelect(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitSelect(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        SparqlQuery q = _pattern.ToQuery();
        foreach (SparqlVariable var in _variables)
        {
            q.AddVariable(var);
        }
        if (_variables.All(v => v.IsResultVariable))
        {
            q.QueryType = SparqlQueryType.SelectAll;
        }
        else
        {
            q.QueryType = SparqlQueryType.Select;
        }
        return q;
    }

    /// <summary>
    /// Throws an error as a Select() cannot be converted back to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown since a Select() cannot be converted back to a Graph Pattern.</exception>
    public GraphPattern ToGraphPattern()
    {
        throw new NotSupportedException("A Select() cannot be converted to a GraphPattern");
    }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new Select(optimiser.Optimise(_pattern), IsSelectAll, _variables);
    }
}