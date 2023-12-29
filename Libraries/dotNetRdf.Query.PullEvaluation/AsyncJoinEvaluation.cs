using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncJoinEvaluation : AbstractAsyncJoinEvaluation
{
    private readonly string[] _joinVars;
    private readonly LinkedList<ISet> _leftSolutions;
    private readonly LinkedList<ISet> _rightSolutions;

    public AsyncJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string[] joinVars)
    :base(lhs, rhs)
    {
        _joinVars = joinVars;
        _leftSolutions = new LinkedList<ISet>();
        _rightSolutions = new LinkedList<ISet>();
    }

    protected override IEnumerable<ISet> ProcessLhs(ISet lhsResult)
    {
        if (_rhsHasMore)
        {
            _leftSolutions.AddLast(lhsResult);
        }

        foreach (ISet joinResult in _rightSolutions
                     .Where(r => lhsResult.IsCompatibleWith(r, _joinVars))
                     .Select(lhsResult.Join))
        {
            yield return joinResult;
        }
    }

    protected override IEnumerable<ISet> ProcessRhs(ISet rhsResult)
    {
        if (_lhsHasMore)
        {
            _rightSolutions.AddLast(rhsResult);
        }

        return _leftSolutions.Where(l => l.IsCompatibleWith(rhsResult, _joinVars))
            .Select(l => l.Join(rhsResult));
    }

    protected override IEnumerable<ISet>? OnLhsDone()
    {
        _rightSolutions.Clear();
        return null;
    }

    protected override IEnumerable<ISet>? OnRhsDone()
    {
        _leftSolutions.Clear();
        return null;
    }
}