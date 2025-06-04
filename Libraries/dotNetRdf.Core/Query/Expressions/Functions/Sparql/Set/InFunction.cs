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
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Expressions.Functions.Sparql.Set;

/// <summary>
/// Class representing the SPARQL IN set function.
/// </summary>
public class InFunction
    : BaseSetFunction
{
    /// <summary>
    /// Creates a new SPARQL IN function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="set">Set.</param>
    public InFunction(ISparqlExpression expr, IEnumerable<ISparqlExpression> set)
        : base(expr, set) { }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessInFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitInFunction(this);
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            return SparqlSpecsHelper.SparqlKeywordIn;
        }
    }

    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator || _expr.Type == SparqlExpressionType.SetOperator) output.Append('(');
        output.Append(_expr);
        if (_expr.Type == SparqlExpressionType.BinaryOperator || _expr.Type == SparqlExpressionType.GraphOperator || _expr.Type == SparqlExpressionType.SetOperator) output.Append(')');
        output.Append(" IN (");
        for (var i = 0; i < _expressions.Count; i++)
        {
            output.Append(_expressions[i]);
            if (i < _expressions.Count - 1)
            {
                output.Append(" , ");
            }
        }
        output.Append(")");
        return output.ToString();
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new InFunction(transformer.Transform(_expr), _expressions.Select(e => transformer.Transform(e)));
    }
}
