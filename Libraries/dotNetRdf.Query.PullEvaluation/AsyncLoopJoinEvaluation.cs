using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncLoopJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, bool leftJoin) : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return leftJoin 
            ? EvaluateLoopLeftJoin(context, input, activeGraph, cancellationToken) 
            : lhs.Evaluate(context, input, activeGraph, cancellationToken).SelectMany(lhsSolution => rhs.Evaluate(context, lhsSolution, activeGraph, cancellationToken));
    }

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
}