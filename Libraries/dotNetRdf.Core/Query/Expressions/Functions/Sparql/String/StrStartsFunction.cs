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

namespace VDS.RDF.Query.Expressions.Functions.Sparql.String;

/// <summary>
/// Represents the SPARQL STRSTARTS Function.
/// </summary>
public class StrStartsFunction
    : BaseBinaryStringFunction
{
    /// <summary>
    /// Creates a new STRSTARTS() function.
    /// </summary>
    /// <param name="stringExpr">String Expression.</param>
    /// <param name="startsExpr">Argument Expression.</param>
    public StrStartsFunction(ISparqlExpression stringExpr, ISparqlExpression startsExpr)
        : base(stringExpr, startsExpr) { }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            return SparqlSpecsHelper.SparqlKeywordStrStarts;
        }
    }

    /// <summary>
    /// Gets the String representation of the Expression.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return SparqlSpecsHelper.SparqlKeywordStrStarts + "(" + _leftExpr + ", " + _rightExpr + ")";
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessStrStartsFunction(this, context, binding);
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitStrStartsFunction(this);
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public override ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new StrStartsFunction(transformer.Transform(_leftExpr), transformer.Transform(_rightExpr));
    }
}
