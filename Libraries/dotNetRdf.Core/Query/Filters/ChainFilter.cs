/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Filters;

/// <summary>
/// Generic Filter for use where multiple Filters are applied on a single Graph Pattern.
/// </summary>
public class ChainFilter : ISparqlFilter
{
    private readonly List<ISparqlFilter> _filters = [];

    /// <summary>
    /// Creates a new Chain Filter.
    /// </summary>
    /// <param name="first">First Filter.</param>
    /// <param name="second">Second Filter.</param>
    public ChainFilter(ISparqlFilter first, ISparqlFilter second)
    {
        _filters.Add(first);
        _filters.Add(second);
    }

    /// <summary>
    /// Creates a new Chain Filter.
    /// </summary>
    /// <param name="filters">Filters.</param>
    public ChainFilter(IEnumerable<ISparqlFilter> filters)
    {
        _filters.AddRange(filters);
    }

    /// <summary>
    /// Creates a new Chain Filter.
    /// </summary>
    /// <param name="first">First Filter.</param>
    /// <param name="rest">Additional Filters.</param>
    public ChainFilter(ISparqlFilter first, IEnumerable<ISparqlFilter> rest)
    {
        _filters.Add(first);
        _filters.AddRange(rest);
    }

    /// <summary>
    /// Adds an additional Filter to the Filter Chain.
    /// </summary>
    /// <param name="filter">A Filter to add.</param>
    protected internal void Add(ISparqlFilter filter)
    {
        _filters.Add(filter);
    }

    /// <summary>
    /// Gets the String representation of the Filters.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        foreach (ISparqlFilter filter in _filters)
        {
            output.Append(filter + " ");
        }
        return output.ToString();
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessChainFilter(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitChainFilter(this);
    }

    /// <summary>
    /// Gets the enumeration of Variables used in the chained Filters.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return from f in _filters
                from v in f.Variables
                select v;
        }
    }

    /// <summary>
    /// Get the filters to be chained.
    /// </summary>
    public IEnumerable<ISparqlFilter> Filters => _filters;

    /// <summary>
    /// Gets the Inner Expression used by the Chained Filters.
    /// </summary>
    /// <remarks>
    /// Equivalent to ANDing all the Chained Filters expressions.
    /// </remarks>
    public ISparqlExpression Expression
    {
        get
        {
            ISparqlExpression expr = new ConstantTerm(new BooleanNode(true));
            if (_filters.Count == 1)
            {
                expr = _filters[0].Expression;
            }
            else if (_filters.Count >= 2)
            {
                expr = new AndExpression(_filters[0].Expression, _filters[1].Expression);
                if (_filters.Count > 2)
                {
                    for (var i = 2; i < _filters.Count; i++)
                    {
                        expr = new AndExpression(expr, _filters[i].Expression);
                    }
                }
            }
            return expr;
        }
    }
}