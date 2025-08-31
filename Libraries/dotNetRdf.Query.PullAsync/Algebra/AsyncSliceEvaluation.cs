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
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.PullAsync;

namespace VDS.RDF.Query.PullAsync.Algebra;

internal class AsyncSliceEvaluation(Slice slice, IAsyncEvaluation inner) : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        if (slice.Limit == 0)
        {
            return new ISet[]{}.ToAsyncEnumerable();
        }
        IAsyncEnumerable<ISet> sliced = inner.Evaluate(context, input, activeGraph, cancellationToken);
        if (slice.Offset > 0) sliced = sliced.Skip(slice.Offset);
        if (slice.Limit >= 0) sliced = sliced.Take(slice.Limit);
        return sliced;
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (slice.Limit == 0)
        {
            yield return [];
        }
        else
        {
            var resultIndex = 0;
            await foreach (IEnumerable<ISet> batch in inner.EvaluateBatch(context, input, activeGraph,
                               cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (resultIndex > slice.Offset + slice.Limit && slice.Limit > 0) yield break;
                if (resultIndex < slice.Offset)
                {
                    resultIndex += batch.Take(slice.Limit).Count();
                }

                if (resultIndex >= slice.Offset)
                {
                    var result = (slice.Limit > 0 ? batch.Take(slice.Limit - (resultIndex - slice.Offset)) : batch).ToList();
                    if (result.Count > 0)
                    {
                        resultIndex += result.Count;
                        yield return result;
                    }
                }
            }
        }
    }
}