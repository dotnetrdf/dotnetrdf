/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2026 dotNetRDF Project (http://dotnetrdf.org/)
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
using VDS.RDF.Query.Grouping;
using VDS.RDF.Query.Pull.Aggregation;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncGroupByEvaluation : IAsyncEvaluation
{
    private readonly GroupBy _groupBy;
    private readonly IAsyncEvaluation _inner;
    private readonly List<string?> _groupVars;
    private readonly Dictionary<GroupingKey, AsyncGroupEvaluation> _groups;
    private readonly List<Func<IAsyncAggregation>> _aggregationProviders = []; 

    public AsyncGroupByEvaluation(GroupBy groupBy, IAsyncEvaluation inner)
    {
        _groupBy = groupBy;
        _inner = inner;
        _groupVars = _groupBy.Grouping?.GroupingKeyNames().ToList() ?? [];
        _groups = [];
    }


    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solutionBinding in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var groupingKey = new GroupingKey(EvaluateGroupingKey(solutionBinding, context, _groupBy.Grouping, activeGraph));
            if (_groups.TryGetValue(groupingKey, out AsyncGroupEvaluation? group))
            {
                group.Accept(solutionBinding, activeGraph);
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

internal class AsyncGroupEvaluation
{
    private List<IAsyncAggregation> _aggregations = [];
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