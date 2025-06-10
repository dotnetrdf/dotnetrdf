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
/// Represents a Zero Length Path in the SPARQL Algebra.
/// </summary>
public class ZeroLengthPath : BasePathOperator
{
    /// <summary>
    /// Creates a new Zero Length Path.
    /// </summary>
    /// <param name="start">Path Start.</param>
    /// <param name="end">Path End.</param>
    /// <param name="path">Property Path.</param>
    public ZeroLengthPath(PatternItem start, PatternItem end, ISparqlPath path)
        : base(start, path, end) { }

    /// <summary>
    /// Return true if both the path start and the path end are fixed terms,
    /// or false if either start or end are a variable.
    /// </summary>
    /// <returns></returns>
    public bool AreBothTerms()
    {
        return PathStart.IsFixed && PathEnd.IsFixed;
    }

    /// <summary>
    /// Return true of the path start and path end reference the same term,
    /// false otherwise.
    /// </summary>
    /// <returns></returns>
    public bool AreSameTerms()
    {
        switch (PathStart)
        {
            case NodeMatchPattern startPattern when PathEnd is NodeMatchPattern endPattern:
                return startPattern.Node.Equals(endPattern.Node);
            case FixedBlankNodePattern startBlankNodePattern when PathEnd is FixedBlankNodePattern endBlankNodePattern:
                return startBlankNodePattern.InternalID.Equals(endBlankNodePattern.InternalID);
            default:
                return false;
        }
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "ZeroLengthPath(" + PathStart + ", " + Path.ToString() + ", " + PathEnd + ")";
    }

    /// <inheritdoc />
    public override T Accept<T>(ISparqlAlgebraVisitor<T> visitor)
    {
        return visitor.VisitZeroLengthPath(this);
    }

    /// <inheritdoc />
    public override TResult Accept<TResult, TContext>(ISparqlQueryAlgebraProcessor<TResult, TContext> processor, TContext context)
    {
        return processor.ProcessZeroLengthPath(this, context);
    }

    /// <summary>
    /// Transforms the Algebra back into a Graph Pattern.
    /// </summary>
    /// <returns></returns>
    public override GraphPattern ToGraphPattern()
    {
        var gp = new GraphPattern();
        var pp = new PropertyPathPattern(PathStart, new FixedCardinality(Path, 0), PathEnd);
        gp.AddTriplePattern(pp);
        return gp;
    }
}
