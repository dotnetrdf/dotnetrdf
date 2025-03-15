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

internal class AsyncLoopJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, bool leftJoin) : IAsyncEvaluation
{
    [Obsolete("Replaced by EvaluateBatch()")]
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return leftJoin 
            ? EvaluateLoopLeftJoin(context, input, activeGraph, cancellationToken) 
            : lhs.Evaluate(context, input, activeGraph, cancellationToken).SelectMany(lhsSolution => rhs.Evaluate(context, lhsSolution, activeGraph, cancellationToken));
    }

    public IAsyncEnumerable<IEnumerable<ISet>> EvaluateBatch(PullEvaluationContext context, IEnumerable<ISet?> input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        return leftJoin
            ? EvaluateLoopLeftJoinBatch(context, input, activeGraph, cancellationToken)
            : lhs.EvaluateBatch(context, input, activeGraph, cancellationToken).SelectMany(lhsSolutions => rhs.EvaluateBatch(context, lhsSolutions, activeGraph, cancellationToken));
    }

    [Obsolete("Replaced by EvaluateLoopLeftJoinBatch()")]
    private async IAsyncEnumerable<ISet> EvaluateLoopLeftJoin(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (ISet lhSolution in lhs.Evaluate(context, input, activeGraph, cancellationToken))
        {
            await using IAsyncEnumerator<ISet> joinEnumerator = rhs.Evaluate(context, lhSolution, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
            if (await joinEnumerator.MoveNextAsync())
            {
                do
                {
                    yield return joinEnumerator.Current;
                } while (await joinEnumerator.MoveNextAsync());
            }
            else
            {
                yield return lhSolution;
            }
        }
    }

    private async IAsyncEnumerable<IEnumerable<ISet>> EvaluateLoopLeftJoinBatch(PullEvaluationContext context,
        IEnumerable<ISet?> input, IRefNode? activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (IEnumerable<ISet> lhSolutionBatch in lhs.EvaluateBatch(context, input, activeGraph,
                           cancellationToken))
        {
            List<ISet> joinResults = new ();
            foreach (ISet lhSolution in lhSolutionBatch)
            {
                await using IAsyncEnumerator<IEnumerable<ISet>> joinEnumerator =
                    rhs.EvaluateBatch(context, lhSolution.AsEnumerable(), activeGraph, cancellationToken)
                        .GetAsyncEnumerator(cancellationToken);
                if (await joinEnumerator.MoveNextAsync())
                {
                    do
                    {
                        joinResults.AddRange(joinEnumerator.Current);
                    } while (await joinEnumerator.MoveNextAsync());
                }
                else
                {
                    joinResults.Add(lhSolution);
                }

                if (joinResults.Count > PullEvaluationContext.DefaultTargetBatchSize)
                {
                    yield return joinResults;
                    joinResults= new List<ISet>();
                }
            }
            if (joinResults.Count > 0) yield return joinResults;
        }
    }
}