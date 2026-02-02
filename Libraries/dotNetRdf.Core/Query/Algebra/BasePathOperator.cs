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
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Abstract Base Class for Path Operators.
/// </summary>
public abstract class BasePathOperator
    : IPathOperator
{
    private readonly HashSet<string> _vars = [];

    /// <summary>
    /// Creates a new Path Operator.
    /// </summary>
    /// <param name="start">Path Start.</param>
    /// <param name="path">Property Path.</param>
    /// <param name="end">Path End.</param>
    protected BasePathOperator(PatternItem start, ISparqlPath path, PatternItem end)
    {
        PathStart = start;
        PathEnd = end;
        Path = path;

        foreach (var v in PathStart.Variables) _vars.Add(v);
        foreach (var v in PathEnd.Variables) _vars.Add(v);
    }

    /// <summary>
    /// Gets the Path Start.
    /// </summary>
    public PatternItem PathStart { get; }

    /// <summary>
    /// Gets the Path End.
    /// </summary>
    public PatternItem PathEnd { get; }

    /// <summary>
    /// Gets the Property Path.
    /// </summary>
    public ISparqlPath Path { get; }

    /// <summary>
    /// Gets the Variables used in the Algebra.
    /// </summary>
    public IEnumerable<string> Variables
    {
        get { return _vars; }
    }

    /// <summary>
    /// Gets the enumeration of fixed variables in the algebra i.e. variables that are guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FixedVariables
    {
        get { return Variables; }
    }

    /// <summary>
    /// Gets the enumeration of floating variables in the algebra i.e. variables that are not guaranteed to have a bound value.
    /// </summary>
    public IEnumerable<string> FloatingVariables
    {
        get { return []; }
    }

    /// <summary>
    /// Transforms the Algebra back into a Query.
    /// </summary>
    /// <returns></returns>
    public SparqlQuery ToQuery()
    {
        var q = new SparqlQuery { RootGraphPattern = ToGraphPattern() };
        q.Optimise();
        return q;
    }

    /// <summary>
    /// Transforms the Algebra back into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public abstract GraphPattern ToGraphPattern();

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public abstract override string ToString();

    /// <inheritdoc />
    public abstract T Accept<T>(ISparqlAlgebraVisitor<T> visitor);

    /// <inheritdoc />
    public abstract TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context);
}