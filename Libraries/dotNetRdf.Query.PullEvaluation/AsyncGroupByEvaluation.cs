using dotNetRdf.Query.PullEvaluation.Aggregation;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Grouping;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncGroupByEvaluation : IAsyncEvaluation
{
    private readonly GroupBy _groupBy;
    private readonly IAsyncEvaluation _inner;
    private readonly List<string?> _groupVars;
    private Dictionary<GroupingKey, AsyncGroupEvaluation> _groups;
    private List<Func<IAsyncAggregation>> _aggregationProviders = new(); 

    internal AsyncGroupByEvaluation(GroupBy groupBy, IAsyncEvaluation inner)
    {
        _groupBy = groupBy;
        _inner = inner;
        _groupVars = _groupBy.Grouping?.GroupingKeyNames().ToList() ?? new List<string?>(0);
        _groups = new Dictionary<GroupingKey, AsyncGroupEvaluation>();
    }


    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solutionBinding in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var groupingKey = new GroupingKey(EvaluateGroupingKey(solutionBinding, context, _groupBy.Grouping, activeGraph));
            if (_groups.ContainsKey(groupingKey))
            {
                _groups[groupingKey].Accept(solutionBinding, activeGraph);
            }
            else
            {
                var age = new AsyncGroupEvaluation(_groupBy, context,
                    _aggregationProviders.Select(provider => provider()));
                age.Accept(solutionBinding, activeGraph);
                _groups.Add(groupingKey, age);
            }
        }

        if (_groups.Count == 0 && _groupBy.Grouping == null)
        {
            var age = new AsyncGroupEvaluation(_groupBy, context, _aggregationProviders.Select(provider => provider()));
            _groups.Add(new GroupingKey(Array.Empty<INode?>()), age);
        }

        foreach (GroupingKey groupingKey in _groups.Keys)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ISet groupResult = groupingKey.ToSet(_groupVars);
            AsyncGroupEvaluation aggregationGroup = _groups[groupingKey];
            ISet aggregationResult = aggregationGroup.GetBindings();
            foreach (var v in aggregationResult.Variables)
            {
                groupResult.Add(v, aggregationResult[v]);
            }

            yield return groupResult;
        }
    }

    public void AddAggregateProvider(Func<IAsyncAggregation> aggregationProvider)
    {
        _aggregationProviders.Add(aggregationProvider);
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

        foreach (var v in grouping.Variables.Where(x => x != grouping.AssignVariable))
        {
            yield return solutionBinding.ContainsVariable(v) ? solutionBinding[v] : null;
        }

        if (grouping.Child != null)
        {
            foreach (INode? v in EvaluateGroupingKey(solutionBinding, context, grouping.Child, activeGraph)) yield return v;
        }        
    }
}

internal class AsyncGroupEvaluation
{
    private List<IAsyncAggregation> _aggregations = new List<IAsyncAggregation>();
    public AsyncGroupEvaluation(GroupBy groupBy, PullEvaluationContext context, IEnumerable<IAsyncAggregation> aggregations)
    {
        _aggregations.AddRange(aggregations);
        foreach (IAsyncAggregation agg in _aggregations)
        {
            agg.Start();
        }
    }

    public void Accept(ISet solutionBinding, IRefNode? activeGraph)
    {
        foreach (IAsyncAggregation agg in _aggregations)
        {
            agg.Accept(new ExpressionContext(solutionBinding, activeGraph));
        }
    }

    public ISet GetBindings()
    {
        ISet ret = new Set();
        foreach (IAsyncAggregation agg in _aggregations)
        {
            agg.End();
            ret.Add(agg.VariableName, agg.Value);
        }

        return ret;
    }
}
internal class GroupingKey : IEquatable<GroupingKey>
{
    private readonly IList<INode?> _bindingsList;
    private readonly int _hashCode;
    public GroupingKey(IEnumerable<INode?> bindings)
    {
        _bindingsList = bindings.ToList();
        _hashCode = CombineHashCodes(_bindingsList);
    }

    private int CombineHashCodes(IEnumerable<INode?> bindings)
    {
        return bindings.Aggregate(17, (current, o) => (31 * current) + o?.GetHashCode() ?? 0);
    }

    public ISet ToSet(IList<string?> varNames)
    {
        ISet ret = new Set();
        for (var i = 0; i < varNames.Count; i++)
        {
            if (varNames[i] != null)
            {
                ret.Add(varNames[i], _bindingsList[i]);
            }
        }

        return ret;
    }

    public override int GetHashCode()
    {
        return _hashCode;
    }

    public override bool Equals(object? obj)
    {
        return obj is GroupingKey other && Equals(other);
    }

    public bool Equals(GroupingKey other)
    {
        if (other._bindingsList.Count != _bindingsList.Count) return false;
        return _bindingsList.SequenceEqual(other._bindingsList, new FastNodeComparer());
    }

    public override string ToString()
    {
        return string.Join(", ", _bindingsList);
    }
}
public static class GroupByExtensions {
    public static IEnumerable<string?> GroupingKeyNames(this ISparqlGroupBy grouping)
    {
        if (grouping.Expression is not null)
        {
            yield return grouping.AssignVariable;
        }
        foreach (var v in grouping.Variables.Where(x => x != grouping.AssignVariable)) {yield return v;}
        if (grouping.Child != null)
        {
            foreach (var v in grouping.Child.GroupingKeyNames()) yield return v;
        }
    }
}
