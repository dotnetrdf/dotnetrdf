using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string[] joinVars)
    : AbstractAsyncJoinEvaluation(lhs, rhs)
{
    private readonly LinkedList<ISet> _leftSolutions = new();
    private readonly LinkedList<ISet> _rightSolutions = new();

    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhsResult, IRefNode? activeGraph)
    {
        if (_rhsHasMore)
        {
            _leftSolutions.AddLast(lhsResult);
        }

        foreach (ISet joinResult in _rightSolutions
                     .Where(r => lhsResult.IsCompatibleWith(r, joinVars))
                     .Select(lhsResult.Join))
        {
            yield return joinResult;
        }
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhsResult, IRefNode? activeGraph)
    {
        if (_lhsHasMore)
        {
            _rightSolutions.AddLast(rhsResult);
        }

        return _leftSolutions.Where(l => l.IsCompatibleWith(rhsResult, joinVars))
            .Select(l => l.Join(rhsResult));
    }

    protected override IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context)
    {
        _rightSolutions.Clear();
        return null;
    }

    protected override IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context)
    {
        _leftSolutions.Clear();
        return null;
    }
}