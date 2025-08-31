using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Pull.Aggregation;

namespace VDS.RDF.Query.Pull.Algebra;

internal class GroupByEvaluation : IEnumerableEvaluation
{
    private readonly GroupBy _groupBy;
    private readonly IEnumerableEvaluation _inner;
    private readonly List<string?> _groupVars;
    private readonly Dictionary<GroupingKey, GroupEvaluation> _groups;
    private readonly List<Func<IAggregateEvaluation>> _aggregationProviders = new(); 

    public GroupByEvaluation(GroupBy groupBy, IEnumerableEvaluation inner)
    {
        _groupBy = groupBy;
        _inner = inner;
        _groupVars = _groupBy.Grouping?.GroupingKeyNames().ToList() ?? new List<string?>(0);
        _groups = new Dictionary<GroupingKey, GroupEvaluation>();
    }


    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        foreach (ISet solutionBinding in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            ApplyGrouping(context, solutionBinding, activeGraph);
        }

        foreach (ISet result in GetGroupResults())
        {
            cancellationToken.ThrowIfCancellationRequested();
            yield return result;
        }
    }

    private IEnumerable<ISet> GetGroupResults()
    {
        if (_groups.Count == 0 && _groupBy.Grouping == null)
        {
            var age = new GroupEvaluation(_aggregationProviders.Select(provider => provider()));
            _groups.Add(new GroupingKey(Array.Empty<INode?>()), age);
        }

        foreach (GroupingKey groupingKey in _groups.Keys)
        {
            ISet groupResult = groupingKey.ToSet(_groupVars);
            GroupEvaluation aggregationGroup = _groups[groupingKey];
            ISet aggregationResult = aggregationGroup.GetBindings();
            foreach (var v in aggregationResult.Variables)
            {
                groupResult.Add(v, aggregationResult[v]);
            }
            yield return groupResult;
        }
    }
    public void AddAggregateProvider(Func<IAggregateEvaluation> aggregationProvider)
    {
        _aggregationProviders.Add(aggregationProvider);
    }

    private void ApplyGrouping(PullEvaluationContext context, ISet solutionBinding, IRefNode? activeGraph)
    {
        var groupingKey = new GroupingKey(EvaluateGroupingKey(solutionBinding, context, _groupBy.Grouping, activeGraph));
        if (_groups.TryGetValue(groupingKey, out GroupEvaluation? group))
        {
            group.Accept(solutionBinding, activeGraph);
        }
        else
        {
            var age = new GroupEvaluation(_aggregationProviders.Select(provider => provider()));
            age.Accept(solutionBinding, activeGraph);
            _groups.Add(groupingKey, age);
        }
    }

    private IEnumerable<INode?> EvaluateGroupingKey(ISet solutionBinding, PullEvaluationContext context,
        ISparqlGroupBy? grouping, IRefNode? activeGraph)
    {
        if (grouping == null) yield break;
        if (grouping.Expression != null)
        {
            INode expressionValue = grouping.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solutionBinding, activeGraph));
            yield return expressionValue;
        }
        else
        {
            foreach (var v in grouping.Variables.Where(x => x != grouping.AssignVariable))
            {
                yield return solutionBinding.ContainsVariable(v) ? solutionBinding[v] : null;
            }
        }

        if (grouping.Child != null)
        {
            foreach (INode? v in EvaluateGroupingKey(solutionBinding, context, grouping.Child, activeGraph)) yield return v;
        }        
    }
}