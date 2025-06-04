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
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Algebra;

/// <summary>
/// Represents a BGP which is a set of Triple Patterns.
/// </summary>
/// <remarks>
/// <para>
/// A Lazy BGP differs from a BGP in that rather than evaluating each Triple Pattern in turn it evaluates across all Triple Patterns.  This is used for queries where we are only want to retrieve a limited number of solutions.
/// </para>
/// <para>
/// A Lazy BGP can only contain concrete Triple Patterns and/or FILTERs and not any of other the specialised Triple Pattern classes.
/// </para>
/// </remarks>
public class LazyBgp
    : Bgp
{
    /// <summary>
    /// Creates a Streamed BGP containing a single Triple Pattern.
    /// </summary>
    /// <param name="p">Triple Pattern.</param>
    public LazyBgp(ITriplePattern p)
    {
        if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern or a Subquery, BIND or FILTER Pattern", "p");
        _triplePatterns.Add(p);
    }

    /// <summary>
    /// Creates a Streamed BGP containing a set of Triple Patterns.
    /// </summary>
    /// <param name="ps">Triple Patterns.</param>
    public LazyBgp(IEnumerable<ITriplePattern> ps)
    {
        if (!ps.All(p => IsLazilyEvaluablePattern(p))) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns or Subquery, BIND, FILTER Patterns", "ps");
        _triplePatterns.AddRange(ps);
    }

    /// <summary>
    /// Creates a Streamed BGP containing a single Triple Pattern.
    /// </summary>
    /// <param name="p">Triple Pattern.</param>
    /// <param name="requiredResults">The number of Results the BGP should attempt to return.</param>
    public LazyBgp(ITriplePattern p, int requiredResults)
    {
        if (!IsLazilyEvaluablePattern(p)) throw new ArgumentException("Triple Pattern instance must be a Triple Pattern, BIND or FILTER Pattern", nameof(p));
        RequiredResults = requiredResults;
        _triplePatterns.Add(p);
    }

    /// <summary>
    /// Creates a Streamed BGP containing a set of Triple Patterns.
    /// </summary>
    /// <param name="ps">Triple Patterns.</param>
    /// <param name="requiredResults">The number of Results the BGP should attempt to return.</param>
    public LazyBgp(IEnumerable<ITriplePattern> ps, int requiredResults)
    {
        if (!ps.All(IsLazilyEvaluablePattern)) throw new ArgumentException("Triple Pattern instances must all be Triple Patterns, BIND or FILTER Patterns", nameof(ps));
        RequiredResults = requiredResults;
        _triplePatterns.AddRange(ps);
    }

    /// <summary>
    /// Get/set the number of results the BGP should attempt to return.
    /// </summary>
    public int RequiredResults { get; set; } = -1;

    private bool IsLazilyEvaluablePattern(ITriplePattern p)
    {
        return p.PatternType is TriplePatternType.Match or TriplePatternType.Filter or TriplePatternType.BindAssignment;
    }

    /// <summary>
    /// Gets the String representation of the Algebra.
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return "LazyBgp(" + RequiredResults + ")";
    }
}