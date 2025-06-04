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

using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Aggregates.Sparql;

/// <summary>
/// Class representing MAX Aggregate Functions.
/// </summary>
public class MaxAggregate
    : BaseAggregate
{
    /// <summary>
    /// Creates a new MAX Aggregate.
    /// </summary>
    /// <param name="expr">Variable Expression.</param>
    /// <param name="distinct">Whether a DISTINCT modifier applies.</param>
    public MaxAggregate(VariableTerm expr, bool distinct)
        : base(expr, distinct)
    {
        Variable = expr.ToString().Substring(1);
    }

    /// <summary>
    /// Creates a new MAX Aggregate.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="distinct">Whether a DISTINCT modifier applies.</param>
    public MaxAggregate(ISparqlExpression expr, bool distinct)
        : base(expr, distinct) { }

    /// <summary>
    /// Creates a new MAX Aggregate.
    /// </summary>
    /// <param name="expr">Variable Expression.</param>
    public MaxAggregate(VariableTerm expr)
        : this(expr, false) { }

    /// <summary>
    /// Creates a new MAX Aggregate.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public MaxAggregate(ISparqlExpression expr)
        : this(expr, false) { }

    /// <summary>
    /// Get the name of the variable that is aggregated if the
    /// aggregation operates on a simple variable term, otherwise
    /// this property will return null.
    /// </summary>
    public string Variable { get; }

    /// <summary>
    /// Creates a new MAX Aggregate.
    /// </summary>
    /// <param name="distinct">Distinct Modifier.</param>
    /// <param name="expr">Expression.</param>
    public MaxAggregate(ISparqlExpression distinct, ISparqlExpression expr)
        : base(expr)
    {
        if (distinct is DistinctModifier)
        {
            _distinct = true;
        }
        else
        {
            throw new RdfQueryException("The first argument to the MaxAggregate constructor must be of type DistinctModifierExpression");
        }
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlAggregateProcessor<TResult, TContext, TBinding> processor, TContext context,
        IEnumerable<TBinding> bindings)
    {
        return processor.ProcessMax(this, context, bindings);
    }

    /// <summary>
    /// Gets the String representation of the Aggregate.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append("MAX(");
        if (_distinct) output.Append("DISTINCT ");
        output.Append(_expr + ")");
        return output.ToString();
    }

    /// <summary>
    /// Gets the Functor of the Aggregate.
    /// </summary>
    public override string Functor
    {
        get
        {
            return SparqlSpecsHelper.SparqlKeywordMax;
        }
    }
}
