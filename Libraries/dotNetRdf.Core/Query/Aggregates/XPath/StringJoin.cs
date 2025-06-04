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

namespace VDS.RDF.Query.Aggregates.XPath;

/// <summary>
/// Represents the XPath fn:string-join() aggregate.
/// </summary>
public class StringJoinAggregate
    : BaseAggregate
{
    /// <summary>
    /// Separator Expression.
    /// </summary>
    protected ISparqlExpression _sep;
    private bool _customSep = true;

    /// <summary>
    /// Creates a new XPath String Join aggregate which uses no separator.
    /// </summary>
    /// <param name="expr">Expression.</param>
    public StringJoinAggregate(ISparqlExpression expr)
        : this(expr, new ConstantTerm(new LiteralNode(string.Empty, false)))
    {
        _customSep = false;
    }

    /// <summary>
    /// Creates a new XPath String Join aggregate.
    /// </summary>
    /// <param name="expr">Expression.</param>
    /// <param name="sep">Separator Expression.</param>
    public StringJoinAggregate(ISparqlExpression expr, ISparqlExpression sep)
        : base(expr)
    {
        _sep = sep;
    }

    /// <summary>
    /// An expression whose value is the string to use to join the values passed into this aggregation.
    /// </summary>
    public ISparqlExpression SeparatorExpression { get => _sep; }


    /// <inheritdoc />
    public override TResult Accept<TResult, TContext, TBinding>(ISparqlAggregateProcessor<TResult, TContext, TBinding> processor, TContext context,
        IEnumerable<TBinding> bindings)
    {
        return processor.ProcessStringJoin(this, context, bindings);
    }

    /// <summary>
    /// Gets the String representation of the function.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append('<');
        output.Append(XPathFunctionFactory.XPathFunctionsNamespace);
        output.Append(XPathFunctionFactory.StringJoin);
        output.Append(">(");
        if (_distinct) output.Append("DISTINCT ");
        output.Append(_expr);
        if (_customSep)
        {
            output.Append(_sep);
        }
        output.Append(')');
        return output.ToString();
    }

    /// <summary>
    /// Gets the Functor of the Expression.
    /// </summary>
    public override string Functor
    {
        get
        {
            return XPathFunctionFactory.XPathFunctionsNamespace + XPathFunctionFactory.StringJoin;
        }
    }
}
