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

using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

/// <summary>
/// Async evaluation of ASK
///
/// Yields a single set empty set if the inner algebra yields any result, otherwise yields nothing.
/// </summary>
internal class AsyncAskEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;

    internal AsyncAskEvaluation(IAsyncEvaluation inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    [Obsolete("Replaced by EvaluateBatch()")]
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<ISet> innerEnumerator = _inner
            .Evaluate(context, input, activeGraph, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        var hasNext = await innerEnumerator.MoveNextAsync();
        if (hasNext) yield return new Set();
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context, IEnumerable<ISet?> batch, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        IList<ISet> results = new List<ISet>();
        foreach (ISet? input in batch)
        {
            cancellationToken.ThrowIfCancellationRequested();
            using IEnumerator<IAsyncEnumerable<IEnumerable<ISet>>> enumerator = _inner.EvaluateBatch(context, input.AsEnumerable(), activeGraph, cancellationToken).AsEnumerable().GetEnumerator();
            var hasNext = enumerator.MoveNext();
            if (hasNext) results.Add(new Set());
        }

        yield return results;
    }
}