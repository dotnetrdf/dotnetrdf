using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncLoopJoinEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _lhs;
    private readonly IAsyncEvaluation _rhs;
    private readonly bool _isLeftJoin;
    
    public AsyncLoopJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, bool leftJoin)
    {
        _lhs = lhs;
        _rhs = rhs;
        _isLeftJoin = leftJoin;
    }

    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return _isLeftJoin 
            ? EvaluateLoopLeftJoin(context, input, activeGraph, cancellationToken) 
            : _lhs.Evaluate(context, input, activeGraph, cancellationToken).SelectMany(lhsSolution => _rhs.Evaluate(context, lhsSolution, activeGraph, cancellationToken));
    }

    private async IAsyncEnumerable<ISet> EvaluateLoopLeftJoin(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (ISet lhSolution in _lhs.Evaluate(context, input, activeGraph, cancellationToken))
        {
            await using IAsyncEnumerator<ISet> joinEnumerator = _rhs.Evaluate(context, lhSolution, activeGraph, cancellationToken).GetAsyncEnumerator(cancellationToken);
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