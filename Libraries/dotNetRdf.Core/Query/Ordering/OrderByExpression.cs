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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Ordering;

/// <summary>
/// An ORDER BY which orders based on the values of a Sparql Expression.
/// </summary>
public class OrderByExpression
    : BaseOrderBy
{
    private readonly ISparqlExpression _expr;

    /// <summary>
    /// Creates a new Order By using the given Expression.
    /// </summary>
    /// <param name="expr">Expression to order by.</param>
    public OrderByExpression(ISparqlExpression expr)
    {
        _expr = expr;
    }

    /// <summary>
    /// Generates a Comparer than can be used to do Ordering based on the given Triple Pattern.
    /// </summary>
    /// <param name="pattern">Triple Pattern.</param>
    /// <param name="nodeComparer">The comparer to use for node ordering.</param>
    /// <returns></returns>
    public override IComparer<Triple> GetComparer(IMatchTriplePattern pattern, ISparqlNodeComparer nodeComparer)
    {
        if (_expr is VariableTerm)
        {
            IComparer<Triple> child = Child?.GetComparer(pattern, nodeComparer);
            Func<Triple, Triple, int> compareFunc = null;
            var var = _expr.Variables.First();
            var comparer = new SparqlOrderingComparer(nodeComparer);
            if (var.Equals(pattern.Subject.Variables.FirstOrDefault()))
            {
                compareFunc = (x, y) => comparer.Compare(x.Subject, y.Subject);
            }
            else if (var.Equals(pattern.Predicate.Variables.FirstOrDefault()))
            {
                compareFunc = (x, y) => comparer.Compare(x.Predicate, y.Predicate);
            }
            else if (var.Equals(pattern.Object.Variables.FirstOrDefault()))
            {
                compareFunc = (x, y) => comparer.Compare(x.Object, y.Object);
            }

            return compareFunc == null ? null : new TripleComparer(compareFunc, Descending, child);
        }

        return null;
    }

    /// <summary>
    /// Gets whether the Ordering is Simple.
    /// </summary>
    public override bool IsSimple
    {
        get
        {
            if (_expr is VariableTerm)
            {
                // An Expression Ordering can be simple if that expression is a Variable Term
                // and the Child Ordering (if any) is simple
                return Child == null || Child.IsSimple;
            }

            return false;
        }
    }

    /// <summary>
    /// Gets all the Variables used in the Ordering.
    /// </summary>
    public override IEnumerable<string> Variables
    {
        get
        {
            return Child != null ? _expr.Variables.Concat(Child.Variables).Distinct() : _expr.Variables;
        }
    }

    /// <summary>
    /// Gets the Expression used for Ordering.
    /// </summary>
    public override ISparqlExpression Expression
    {
        get
        {
            return _expr;
        }
    }

    /// <summary>
    /// Gets the String representation of the Order By.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        var output = new StringBuilder();
        output.Append(Descending ? "DESC(" : "ASC(");
        output.Append(_expr);
        output.Append(")");

        if (Child != null)
        {
            output.Append(" ");
            output.Append(Child);
        }
        else
        {
            output.Append(" ");
        }

        return output.ToString();
    }
}