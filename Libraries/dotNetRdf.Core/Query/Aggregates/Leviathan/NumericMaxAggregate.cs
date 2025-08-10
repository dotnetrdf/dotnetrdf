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

namespace VDS.RDF.Query.Aggregates.Leviathan;

/// <summary>
/// Class representing NMAX Aggregate Functions.
/// </summary>
/// <remarks>
/// Only operates over numeric data which is typed to one of the supported SPARQL Numeric types (integers, decimals and doubles).
/// </remarks>
public class NumericMaxAggregate 
    : BaseAggregate
{
    /// <summary>
    /// Creates a new NMAX Aggregate.
    /// </summary>
    /// <param name="expr">Variable Expression.</param>
    public NumericMaxAggregate(VariableTerm expr)
        : this(expr, false) { }

    /// <summary>
    /// Creates a new NMAX Aggregate.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public NumericMaxAggregate(ISparqlExpression expr)
        : this(expr, false) { }

    /// <summary>
    /// Creates a new NMAX Aggregate.
    /// </summary>
    /// <param name="expr">Variable Expression.</param>
    /// <param name="distinct">Whether a DISTINCT modifier applies.</param>
    public NumericMaxAggregate(VariableTerm expr, bool distinct)
        : base(expr, distinct)
    {
        Variable = expr.ToString().Substring(1);
    }

    /// <summary>
    /// Creates a new NMAX Aggregate.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="distinct">Whether a DISTINCT modifier applies.</param>
    public NumericMaxAggregate(ISparqlExpression expr, bool distinct)
        : base(expr, distinct) { }

    /// <summary>
    /// The variable to aggregate.
    /// </summary>
    public string Variable { get; }


    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlAggregateProcessor<TResult, TContext, TBinding> processor, TContext context,
        IEnumerable<TBinding> bindings)
    {
        return processor.ProcessNumericMax(this, context, bindings);
    }

    /// <summary>
    /// Gets the String representation of the Aggregate.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append('<');
        output.Append(LeviathanFunctionFactory.LeviathanFunctionsNamespace);
        output.Append(LeviathanFunctionFactory.NumericMax);
        output.Append(">(");
        if (_distinct) output.Append("DISTINCT ");
        output.Append(_expr);
        output.Append(')');
        return output.ToString();
    }

    /// <summary>
    /// Gets the Functor of the Aggregate.
    /// </summary>
    public override string Functor
    {
        get
        {
            return LeviathanFunctionFactory.LeviathanFunctionsNamespace + LeviathanFunctionFactory.NumericMax;
        }
    }
}
