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
/// Represents a Reduced modifier on a SPARQL Query.
/// </summary>
public class Reduced 
    : IUnaryOperator
{
    /// <summary>
    /// Creates a new Reduced Modifier.
    /// </summary>
    /// <param name="pattern">Pattern.</param>
    public Reduced(ISparqlAlgebra pattern)
    {
        InnerAlgebra = pattern;
    }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return InnerAlgebra.Variables.Distinct();
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables { get { return InnerAlgebra.FloatingVariables; } }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return InnerAlgebra.FixedVariables; } }

    /// <summary>
    /// Gets the Inner Algebra.
    /// </summary>
    public ISparqlAlgebra InnerAlgebra { get; }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Reduced(" + InnerAlgebra + ")";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessReduced(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitReduced(this);
    }

    /// <summary>
    /// Converts the Algebra back to a SPARQL Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        SparqlQuery q = InnerAlgebra.ToQuery();
        switch (q.QueryType)
        {
            case SparqlQueryType.Select:
                q.QueryType = SparqlQueryType.SelectReduced;
                break;
            case SparqlQueryType.SelectAll:
                q.QueryType = SparqlQueryType.SelectAllReduced;
                break;
            case SparqlQueryType.SelectAllDistinct:
            case SparqlQueryType.SelectAllReduced:
            case SparqlQueryType.SelectDistinct:
            case SparqlQueryType.SelectReduced:
                throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query that already has a DISTINCT/REDUCED modifier applied");
            case SparqlQueryType.Ask:
            case SparqlQueryType.Construct:
            case SparqlQueryType.Describe:
            case SparqlQueryType.DescribeAll:
                throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query that is not a SELECT query");
            case SparqlQueryType.Unknown:
                q.QueryType = SparqlQueryType.SelectReduced;
                break;
            default:
                throw new NotSupportedException("Cannot convert a Reduced() to a SPARQL Query when the Inner Algebra converts to a Query with an unexpected Query Type");
        }
        return q;
    }

    /// <summary>
    /// Throws an exception since a Reduced() cannot be converted back to a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">Thrown since a Reduced() cannot be converted to a Graph Pattern.</exception>
    public GraphPattern ToGraphPattern()
    {
        throw new NotSupportedException("A Reduced() cannot be converted to a GraphPattern");
    }

    /// <summary>
    /// Transforms the Inner Algebra using the given Optimiser.
    /// </summary>
    /// <param name="optimiser">Optimiser.</param>
    /// <returns></returns>
    public ISparqlAlgebra Transform(IAlgebraOptimiser optimiser)
    {
        return new Reduced(optimiser.Optimise(InnerAlgebra));
    }
}