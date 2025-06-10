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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a sub-query as an Algebra operator (only used when strict algebra is generated).
/// </summary>
public class SubQuery : ITerminalOperator
{
    /// <summary>
    /// Get the sub-query.
    /// </summary>
    public SparqlQuery Query { get; }

    /// <summary>
    /// Creates a new sub-query operator.
    /// </summary>
    /// <param name="q">Sub-query.</param>
    public SubQuery(SparqlQuery q)
    {
        Query = q;
    }

    /// <summary>
    /// Gets the variables used in the sub-query which are projected out of it.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get 
        { 
            return Query.Variables.Where(v => v.IsResultVariable).Select(v => v.Name); 
        }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables { get { return Query.ToAlgebra().FloatingVariables; } }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables { get { return Query.ToAlgebra().FixedVariables; } }

    /// <summary>
    /// Converts the algebra back into a Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        var q = new SparqlQuery { RootGraphPattern = ToGraphPattern() };
        return q;
    }

    /// <summary>
    /// Converts the algebra back into a sub-query.
    /// </summary>
    /// <returns></returns>
    public GraphPattern ToGraphPattern()
    {
        var gp = new GraphPattern();
        gp.TriplePatterns.Add(new SubQueryPattern(Query));
        return gp;
    }

    /// <summary>
    /// Gets the string representation of the algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "Subquery()";
    }

    /// <inheritdoc />
    public TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessSubQuery(this, context);
    }

    /// <inheritdoc />
    public T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitSubQuery(this);
    }
}
