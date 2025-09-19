using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncCrossProductJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs) : AbstractAsyncJoinEvaluation(lhs, rhs)
{
    private LinkedList<ISet>? _lhsSolutions = new();
    private LinkedList<ISet>? _rhsSolutions = new();

    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhSolution, IRefNode? activeGraph)
    {
        _lhsSolutions?.AddLast(lhSolution);
        return _rhsSolutions?.Select(lhSolution.Join) ?? [];
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph)
    {
        _rhsSolutions?.AddLast(rhSolution);
        return _lhsSolutions?.Select(x => x.Join(rhSolution)) ?? [];
    }

    protected override IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context)
    {
        // No longer need to keep RHS solutions
        _rhsSolutions?.Clear();
        _rhsSolutions = null;
        return null;
    }

    protected override IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context)
    {
        _lhsSolutions?.Clear();
        _lhsSolutions = null;
        return null;
    }
}