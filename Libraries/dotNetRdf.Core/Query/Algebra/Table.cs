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

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a fixed set of solutions.
/// </summary>
public class Table
    : ITerminalOperator
{
    /// <summary>
    /// Creates a new fixed set of solutions.
    /// </summary>
    /// <param name="table">Table.</param>
    public Table(BaseMultiset table)
    {
        Multiset = table ?? throw new ArgumentNullException(nameof(table));
    }

    /// <summary>
    /// Returns the fixed set of solutions.
    /// </summary>
    /// <param name="context">Evaluation Context.</param>
    /// <returns></returns>
    public BaseMultiset Evaluate(SparqlEvaluationContext context)
    {
        context.OutputMultiset = Multiset;
        return context.OutputMultiset;
    }

    /// <summary>
    /// Get the fixed set of solutions that this algebra represents.
    /// </summary>
    public BaseMultiset Multiset { get; }

    /// <summary>
    /// Gets the variables used in the algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get
        {
            return Multiset.Variables; 
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get
        {
            // Floating variables are any where there are rows with an unbound value
            return Multiset.Variables.Where(v => Multiset.Sets.Any(s => s[v] == null));
        }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get
        {
            // Fixed variables are any where there are no rows with an unbound value
            return Multiset.Variables.Where(v => Multiset.Sets.All(s => s[v] != null));
        }
    }

    /// <summary>
    /// Throws an error as this cannot be converted back into a query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        throw new NotSupportedException("Cannot convert Table to Query");
    }

    /// <summary>
    /// Throws an error as this cannot be converted back into a graph pattern.
    /// </summary>
    /// <returns></returns>
    public Patterns.GraphPattern ToGraphPattern()
    {
        throw new NotSupportedException("Cannot convert Table to Graph Pattern");
    }

    /// <summary>
    /// Gets the string representation of the algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Table()";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessUnknownOperator(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitUnknownOperator(this);
    }
}
