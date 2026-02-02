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

using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Pull.Algebra;

/// <summary>
/// Implements evaluation of a TriplePattern against a PullEvaluationContext returning all matching solution bindings.
/// </summary>
internal class AsyncTriplePatternEvaluation : IAsyncEvaluation
{
    private readonly IMatchTriplePattern _triplePattern;

    /// <summary>
    /// Construct a new triple pattern evaluator
    /// </summary>
    /// <param name="triplePattern"></param>
    public AsyncTriplePatternEvaluation(IMatchTriplePattern triplePattern)
    {
        _triplePattern = triplePattern;
    }


    /// <summary>
    /// Evaluate the triple pattern optionally using the given input solution.
    /// </summary>
    /// <param name="context">Evaluation context</param>
    /// <param name="input">Optional input solution</param>
    /// <param name="activeGraph">Optional active graph name</param>
    /// <param name="cancellationToken">Cancellation token for the enumeration</param>
    /// <returns></returns>
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return context.GetTriples(_triplePattern, input, activeGraph)
            .Select(t=>_triplePattern.Evaluate(context,t))
            .Where(set => set != null)
            .Select(set => input != null ? set.Join(input) : set)
            .ToAsyncEnumerable();
    }
}