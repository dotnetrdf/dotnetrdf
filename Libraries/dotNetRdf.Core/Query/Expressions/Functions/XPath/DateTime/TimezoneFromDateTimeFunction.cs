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

namespace VDS.RDF.Query.Expressions.Functions.XPath.DateTime;

/// <summary>
/// Represents the XPath timezone-from-dateTime() function.
/// </summary>
public class TimezoneFromDateTimeFunction
    : ISparqlExpression
{
    /// <summary>
    /// Expression that the Function applies to.
    /// </summary>
    protected ISparqlExpression _expr;

    /// <summary>
    /// Creates a new XPath Timezone from Date Time function.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public TimezoneFromDateTimeFunction(ISparqlExpression expr)
    {
        _expr = expr;
    }

    /// <summary>
    /// Get the expression that evaluates to the datetime value to process.
    /// </summary>
    public ISparqlExpression InnerExpression { get => _expr; }


    /// <inheritdoc />
    public virtual TResult Accept<TResult, TContext, TBinding>(ISparqlExpressionProcessor<TResult, TContext, TBinding> processor, TContext context, TBinding binding)
    {
        return processor.ProcessTimezoneFromDateTimeFunction(this, context, binding);
    }

    /// <inheritdoc />
    public virtual T Accept<T>(ISparqlExpressionVisitor<T> visitor)
    {
        return visitor.VisitTimezoneFromDateTimeFunction(this);
    }

    /// <summary>
    /// Gets the Variables used in the function.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return _expr.Variables;
        }
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "<" + XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime + ">(" + _expr + ")";
    }

    /// <summary>
    /// Gets the Type of the Expression.
    /// </summary>
    public SparqlExpressionType Type
    {
        get
        {
            return SparqlExpressionType.Function;
        }
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public virtual string Functor
    {
        get
        {
            return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.TimezoneFromDateTime;
        }
    }

    /// <summary>
    /// Gets the Arguments of the Expression.
    /// </summary>
    public IEnumerable<ISparqlExpression> Arguments
    {
        get
        {
            return _expr.AsEnumerable();
        }
    }

    /// <summary>
    /// Gets whether an expression can safely be evaluated in parallel.
    /// </summary>
    public virtual bool CanParallelise
    {
        get
        {
            return _expr.CanParallelise;
        }
    }

    /// <summary>
    /// Transforms the Expression using the given Transformer.
    /// </summary>
    /// <param name="transformer">Expression Transformer.</param>
    /// <returns></returns>
    public virtual ISparqlExpression Transform(IExpressionTransformer transformer)
    {
        return new TimezoneFromDateTimeFunction(transformer.Transform(_expr));
    }

    
}
