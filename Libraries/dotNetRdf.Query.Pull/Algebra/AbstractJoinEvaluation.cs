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

using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal abstract class AbstractJoinEvaluation(IEnumerableEvaluation lhs, IEnumerableEvaluation rhs) : IEnumerableEvaluation
{
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph = null, CancellationToken cancellationToken = default)
    {
        var lhsHasMore = true;
        var rhsHasMore = true;
        using IEnumerator<ISet> lhsResults = lhs.Evaluate(context, input, activeGraph, cancellationToken).GetEnumerator();
        using IEnumerator<ISet> rhsResults = rhs.Evaluate(context, input, activeGraph, cancellationToken).GetEnumerator();
        IEnumerable<ISet>? joinEnumerator = null;
        var pullLeft = true;
        do
        {
            if (joinEnumerator != null)
            {
                foreach (ISet? joinResult in joinEnumerator)
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
                    if (pullLeft)
                    {
                        lhsHasMore = lhsResults.MoveNext();
                        if (lhsHasMore && lhsResults.Current != null)
                        {
                            foreach (ISet p in ProcessLhs(context, lhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                            {
                                yield return p;
                            }
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults =  OnLhsDone(context);
                            if (moreResults != null)
                            {
                                foreach (ISet r in moreResults)
                                {
                                    yield return r;
                                }
                            }
                        }
                        pullLeft = false;
                    }
                    else
                    {
                        rhsHasMore = rhsResults.MoveNext();
                        if (rhsHasMore && rhsResults.Current != null )
                        {
                            foreach (ISet p in ProcessRhs(context, rhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                            {
                                yield return p;
                            }
                        }
                        else
                        {
                            IEnumerable<ISet>? moreResults = OnRhsDone(context);
                            if (moreResults != null)
                            {
                                foreach (ISet r in moreResults) { yield return r; }
                            }
                        }
                        pullLeft = true;
                    }
                }
                else
                {
                    lhsHasMore = lhsResults.MoveNext();
                    if (lhsHasMore && lhsResults.Current != null)
                    {
                        foreach (ISet p in ProcessLhs(context, lhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                        {
                            yield return p;
                        }
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
                rhsHasMore = rhsResults.MoveNext();
                if (rhsHasMore && rhsResults.Current != null)
                {
                    foreach (ISet p in ProcessRhs(context, rhsResults.Current, activeGraph, lhsHasMore, rhsHasMore))
                    {
                        yield return p;
                    }
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


    protected abstract IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhSolution, IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore);
    protected abstract IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph, bool lhsHaMore, bool rhsHaMore);
    protected abstract IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context);
    protected abstract IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context);
}