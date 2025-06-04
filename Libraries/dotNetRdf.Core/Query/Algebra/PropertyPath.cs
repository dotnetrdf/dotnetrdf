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

using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents an arbitrary property path in the algebra (only used when strict algebra is generated).
/// </summary>
public class PropertyPath
    : BasePathOperator, ITerminalOperator
{
    /// <summary>
    /// Creates a new Property Path operator.
    /// </summary>
    /// <param name="start">Path Start.</param>
    /// <param name="path">Path Expression.</param>
    /// <param name="end">Path End.</param>
    public PropertyPath(PatternItem start, ISparqlPath path, PatternItem end)
        : base(start, path, end) { }

    /// <summary>
    /// Converts the algebra back into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public override GraphPattern ToGraphPattern()
    {
        var gp = new GraphPattern();
        gp.AddTriplePattern(new PropertyPathPattern(PathStart, Path, PathEnd));
        return gp;
    }

    /// <summary>
    /// Gets the string representation of the algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "PropertyPath()";
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitPropertyPath(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessPropertyPath(this, context);
    }
}
