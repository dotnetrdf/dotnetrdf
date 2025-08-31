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

namespace VDS.RDF.Query.PullAsync.Algebra;

internal abstract class AbstractAsyncJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs) : IAsyncEvaluation
{
    [Obsolete("Replaced by EvaluateBatch()")]
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lhsHasMore = true;
        var rhsHasMore = true;
        IAsyncEnumerator<ISet> lhsResults = lhs.Evaluate(context, input, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
        IAsyncEnumerator<ISet> rhsResults = rhs.Evaluate(context, input, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
        Task<bool> lhsMoveNext = lhsResults.MoveNextAsync().AsTask();
        Task<bool> rhsMoveNext = rhsResults.MoveNextAsync().AsTask();
        IAsyncEnumerable<ISet>? joinEnumerator = null;
        do
        {
            if (joinEnumerator != null)
            {
                await foreach (ISet? joinResult in joinEnumerator)
                {
                    if (joinResult != null)
                    {
                        yield return joinResult;
                    }
                }

                joinEnumerator = null;
            }

            if (lhsHasMore)
            {
                if (rhsHasMore)
                {
                    var completed = Task.WaitAny(lhsMoveNext, rhsMoveNext);
                    if (completed == 0)
                    {
                        lhsHasMore = lhsMoveNext.Result;
                        if (lhsHasMore)
                        {
                            foreach (ISet p in ProcessLhs(context, lhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                            {
                                yield return p;
                            }

                            lhsMoveNext = lhsResults.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults =  OnLhsDone(context);
                            if (moreResults != null)
                            {
                                foreach (ISet r in moreResults) { yield return r; }
                            }
                        }
                    }
                    else
                    {
                        rhsHasMore = rhsMoveNext.Result;
                        if (rhsHasMore)
                        {
                            foreach (ISet p in ProcessRhs(context, rhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                            {
                                yield return p;
                            }

                            rhsMoveNext = rhsResults.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults = OnRhsDone(context);
                            if (moreResults != null)
                            {
                                foreach (ISet r in moreResults) { yield return r; }
                            }
                        }
                    }
                }
                else
                {
                    lhsHasMore = await lhsMoveNext;
                    if (lhsHasMore)
                    {
                        foreach (ISet p in ProcessLhs(context, lhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                        {
                            yield return p;
                        }

                        lhsMoveNext = lhsResults.MoveNextAsync().AsTask();
                    }
                    else
                    {
                        IEnumerable<ISet>? moreResults = OnLhsDone(context);
                        if (moreResults != null)
                        {
                            foreach (ISet r in moreResults) { yield return r; }
                        }
                    }
                }
            }
            else if (rhsHasMore)
            {
                rhsHasMore = await rhsMoveNext;
                if (rhsHasMore)
                {
                    foreach (ISet p in ProcessRhs(context, rhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                    {
                        yield return p;
                    }

                    rhsMoveNext = rhsResults.MoveNextAsync().AsTask();
                }
                else
                {
                    IEnumerable<ISet>? moreResults = OnRhsDone(context);
                    if (moreResults != null)
                    {
                        foreach (ISet r in moreResults) { yield return r; }
                    }
                }
            }
        } while (lhsHasMore || rhsHasMore);
    }

    public async IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context,
        IEnumerable<ISet?> input,
        IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var lhsHasMore = true;
        var rhsHasMore = true;
        IList<ISet?> inputBatch = input.ToList();
        IAsyncEnumerator<IEnumerable<ISet>> lhsBatches = lhs.EvaluateBatch(context, inputBatch, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
        IAsyncEnumerator<IEnumerable<ISet>> rhsBatches = rhs.EvaluateBatch(context, inputBatch, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
        Task<bool> lhsMoveNext = lhsBatches.MoveNextAsync().AsTask();
        Task<bool> rhsMoveNext = rhsBatches.MoveNextAsync().AsTask();
        do
        {
            if (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            if (lhsHasMore)
            {
                if (rhsHasMore)
                {
                    var completed = Task.WaitAny(lhsMoveNext, rhsMoveNext);
                    if (completed == 0)
                    {
                        lhsHasMore = lhsMoveNext.Result;
                        if (lhsHasMore)
                        {
                            yield return ProcessLhs(context, lhsBatches.Current, activeGraph, lhsHasMore, rhsHasMore);
                            lhsMoveNext = lhsBatches.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults = OnLhsDone(context);
                            if (moreResults != null)
                            {
                                yield return moreResults;
                            }
                        }
                    }
                    else
                    {
                        rhsHasMore = rhsMoveNext.Result;
                        if (rhsHasMore)
                        {
                            yield return ProcessRhs(context, rhsBatches.Current, activeGraph, lhsHasMore, rhsHasMore);
                            rhsMoveNext = rhsBatches.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults = OnRhsDone(context);
                            if (moreResults != null)
                            {
                                yield return moreResults;
                            }
                        }
                    }
                }
                else
                {
                    lhsHasMore = await lhsMoveNext;
                    if (lhsHasMore)
                    {
                        yield return ProcessLhs(context, lhsBatches.Current, activeGraph, lhsHasMore, rhsHasMore);
                        lhsMoveNext = lhsBatches.MoveNextAsync().AsTask();
                    }
                    else
                    {
                        IEnumerable<ISet>? moreResults = OnLhsDone(context);
                        if (moreResults != null)
                        {
                            yield return moreResults;
                        }
                    }
                }
            } else if (rhsHasMore)
            {
                rhsHasMore = await rhsMoveNext;
                if (rhsHasMore)
                {
                    yield return ProcessRhs(context, rhsBatches.Current, activeGraph, lhsHasMore, rhsHasMore);
                    rhsMoveNext = rhsBatches.MoveNextAsync().AsTask();
                }
                else
                {
                    IEnumerable<ISet>? moreResults = OnRhsDone(context);
                    if (moreResults != null)
                    {
                        yield return moreResults;
                    }
                }
            }
        } while(lhsHasMore || rhsHasMore);
    }

    protected abstract IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhSolution, IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore);
    protected abstract IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore);
    protected abstract IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context);
    protected abstract IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context);

    protected virtual IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, IEnumerable<ISet> batch,
        IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore)
    {
        return batch.SelectMany(s=>ProcessLhs(context, s, activeGraph, lhsHaMore, rhsHaMore));
    }

    protected virtual IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, IEnumerable<ISet> batch,
        IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore)
    {
        return batch.SelectMany(s=>ProcessRhs(context, s, activeGraph, lhsHaMore, rhsHaMore));
    }
}