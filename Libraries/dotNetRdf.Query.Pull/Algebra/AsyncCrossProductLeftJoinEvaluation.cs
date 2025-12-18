using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;
using VDS.RDF.Query.Pull;
using VDS.RDF.Query.Pull.Algebra;

internal class AsyncCrossProductLeftJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, ISparqlFilter filter) : AbstractAsyncJoinEvaluation(lhs, rhs)
{
    private readonly List<ISet> _leftSolutions = [];
    private readonly List<ISet> _rightSolutions = [];
    private readonly HashSet<int> _unjoinedLeftSolutions = [];

    protected override IEnumerable<ISet> ProcessLhs(PullEvaluationContext context, ISet lhSolution, IRefNode? activeGraph)
    {
        Func<ISet, bool>? filterFunc = filter == null ? null : (s => Filter(s, context, activeGraph));

        if (_rhsHasMore)
        {
            // Keep extending the list of left solutions to join with new right solutions
            _leftSolutions.Add(lhSolution);
        }
        var joinedSolutions = _rightSolutions.Select(s=>lhSolution.Join(s))
            .Where(s => filterFunc == null || filterFunc(s))
            .ToList();
        if (joinedSolutions.Count > 0) return joinedSolutions;
        _unjoinedLeftSolutions.Add(_leftSolutions.Count - 1);
        return [];
    }

    protected override IEnumerable<ISet> ProcessRhs(PullEvaluationContext context, ISet rhSolution, IRefNode? activeGraph)
    {
        if (_lhsHasMore)
        {
            // Keep extending the list of right solutions to join with new left solutions
            _rightSolutions.Add(rhSolution);
        }
        for (var i = 0; i < _leftSolutions.Count; i++)
        {
            var joinedSolution = _leftSolutions[i].Join(rhSolution);
            if (filter == null || Filter(joinedSolution, context, activeGraph))
            {
                _unjoinedLeftSolutions.Remove(i);
                yield return joinedSolution;
            }
        }
    }

    protected override IEnumerable<ISet>? OnLhsDone(PullEvaluationContext context)
    {
        // No longer need the right solution list
        _rightSolutions.Clear();
        if (!_rhsHasMore)
        {
            // If there are no more right solutions, we can return any left solutions that didn't join
            foreach (var i in _unjoinedLeftSolutions)
            {
                yield return _leftSolutions[i];
            }
        }
    }

    protected override IEnumerable<ISet>? OnRhsDone(PullEvaluationContext context)
    {
        // Now we need to return any left solutions that did not join with anything on the right
        foreach (var i in _unjoinedLeftSolutions)
        {
            yield return _leftSolutions[i];
        }
        // Clean up left solution lists
        _unjoinedLeftSolutions.Clear();
        _leftSolutions.Clear();
    }

    private bool Filter(ISet s, PullEvaluationContext context, IRefNode? activeGraph)
    {
        if (filter == null) return true;
        try
        {
            return filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(s, activeGraph)).AsSafeBoolean();
        }
        catch (RdfQueryException)
        {
            return false;
        }
    }
}