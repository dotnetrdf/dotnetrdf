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

using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using VDS.RDF.Query.Pull.Aggregation;
using VDS.RDF.Query.Pull.Algebra;
using VDS.RDF.Query.Pull.Paths;

namespace VDS.RDF.Query.Pull;

internal class EvaluationBuilder
{
    public IEnumerableEvaluation Build(ISparqlAlgebra algebra, PullEvaluationContext context)
    {
        return algebra switch
        {
            Filter filter => BuildFilter(filter, context),
            Query.Algebra.Graph graph => BuildGraph(graph, context),
            IBgp bgp => BuildBgp(bgp, context),
            Join join => BuildJoin(join, context),
            LeftJoin leftJoin => BuildLeftJoin(leftJoin, context),
            Select select => BuildSelect(select, context),
            Union union => BuildUnion(union, context),
            Ask ask => BuildAsk(ask, context),
            AskAnyTriples => BuildAskAnyTriples(),
            Distinct distinct => BuildDistinct(distinct, context),
            OrderBy orderBy => BuildOrderBy(orderBy, context),
            Slice slice => BuildSlice(slice, context),
            Extend extend => BuildExtend(extend, context),
            Reduced reduced => BuildReduced(reduced, context),
            Bindings bindings => BuildBindings(bindings),
            GroupBy groupBy => BuildGroupBy(groupBy, context),
            Having having => BuildHaving(having, context),
            SubQuery subQuery => BuildSubQuery(subQuery, context),
            Minus minus => BuildMinus(minus, context),
            _ => throw new RdfQueryException($"Unsupported query algebra ({algebra.GetType()}: {algebra}")
        };
    }

    private IEnumerableEvaluation BuildLeftJoin(ILeftJoin leftJoin, PullEvaluationContext context)
    {
        IEnumerableEvaluation lhs = Build(leftJoin.Lhs, context);
        IEnumerableEvaluation rhs = Build(leftJoin.Rhs, context);
        var joinVars = new HashSet<string>(leftJoin.Lhs.Variables);
        joinVars.IntersectWith(new HashSet<string>(leftJoin.Rhs.Variables));
        var joinEval = new LeftJoinEvaluation(
            lhs,
            rhs,
            joinVars.ToArray(),
            leftJoin.Rhs.Variables.ToArray(),
            leftJoin.Filter
            );
        return joinEval;
    }
    
    private IEnumerableEvaluation BuildTriplePattern(ITriplePattern triplePattern, PullEvaluationContext context)
    {
        if (triplePattern is IMatchTriplePattern matchTriplePattern)
        {
            return new TriplePatternEvaluation(matchTriplePattern);
        }

        if (triplePattern is BindPattern bindPattern)
        {
            if (bindPattern.InnerExpression.Variables.Any())
            {
                throw new RdfQueryException("Cannot process BIND with variables as first triple pattern");
            }

            return new BindPatternEvaluation(bindPattern, null);
        }

        if (triplePattern is SubQueryPattern subQueryPattern)
        {
            return BuildSubQueryPattern(subQueryPattern, context);
        }

        if (triplePattern is PropertyPathPattern ppPattern)
        {
            return BuildPropertyPathPattern(ppPattern, context);
        }
        throw new RdfQueryException($"Unsupported triple pattern algebra ({triplePattern.GetType()}): {triplePattern}");
    }

    private IEnumerableEvaluation BuildJoin(Join join, PullEvaluationContext context)
    {
        ISet<string> joinVars = new HashSet<string>(join.Lhs.Variables);
        joinVars.IntersectWith(join.Rhs.Variables);
        return new JoinEvaluation(Build(join.Lhs, context), Build(join.Rhs, context), joinVars.ToArray());
    }

    private IEnumerableEvaluation BuildBgp(IBgp bgp, PullEvaluationContext context)
    {
        if (bgp.TriplePatterns.Count == 0)
        {
            return new IdentityEvaluation();
        }
        ISet<string> boundVars = new HashSet<string>(bgp.TriplePatterns[0].Variables);
        IEnumerableEvaluation result = BuildTriplePattern(bgp.TriplePatterns[0], context);
        for (var i = 1; i < bgp.TriplePatterns.Count; i++)
        {
            ITriplePattern tp = bgp.TriplePatterns[i];
            ISet<string> joinVars = new HashSet<string>(boundVars);
            joinVars.IntersectWith(bgp.Variables);
            boundVars.UnionWith(bgp.Variables);
            result = tp switch
            {
                FilterPattern fp => new SparqlFilterEvaluation(fp.Filter, result, true),
                BindPattern bp => new BindPatternEvaluation(bp, result),
                _ => new JoinEvaluation(result, BuildTriplePattern(tp, context), joinVars.ToArray())
            };
        }
        return result;
    }

    private IEnumerableEvaluation BuildSelect(Select select, PullEvaluationContext context)
    {
        if (select.IsSelectAll) { return Build(select.InnerAlgebra, context); }

        return new SelectEvaluation(select, Build(select.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildFilter(Filter filter, PullEvaluationContext context)
    {
        return new FilterEvaluation(filter, Build(filter.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildUnion(Union union, PullEvaluationContext context)
    {
        return new UnionEvaluation(Build(union.Lhs, context), Build(union.Rhs, context));
    }

    private IEnumerableEvaluation BuildGraph(Query.Algebra.Graph graph, PullEvaluationContext context)
    {
        if (graph.GraphSpecifier.TokenType == Token.VARIABLE)
        {
            return new GraphEvaluation(graph.GraphSpecifier.Value.TrimStart('?'), Build(graph.InnerAlgebra, context));
        }

        if (graph.GraphSpecifier.TokenType == Token.URI)
        {
            // TODO: Use URI factory and node factory from context
            var graphName = new UriNode(new Uri(graph.GraphSpecifier.Value));
            return new GraphEvaluation(graphName, Build(graph.InnerAlgebra, context));
        }
        throw new RdfQueryException($"Unsupported graph specifier token {graph.GraphSpecifier}");
    }

    private IEnumerableEvaluation BuildAsk(Ask ask, PullEvaluationContext context)
    {
        return new AskEvaluation(Build(ask.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildAskAnyTriples()
    {
        return new AskAnyTriplesEvaluation();
    }

    private IEnumerableEvaluation BuildDistinct(Distinct distinct, PullEvaluationContext context)
    {
        return new DistinctEvaluation(Build(distinct.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildOrderBy(OrderBy orderBy, PullEvaluationContext context)
    {
        return new OrderByEvaluation(orderBy, Build(orderBy.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildSlice(Slice slice, PullEvaluationContext context)
    {
        if (slice.Limit > 0)
        {
            context.SliceLength = slice.Limit;
            if (slice.Offset > 0) context.SliceLength += slice.Offset;
        }

        return new SliceEvaluation(slice, Build(slice.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildExtend(Extend extend, PullEvaluationContext context)
    {
        return new ExtendEvaluation(extend, Build(extend.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildReduced(Reduced reduced, PullEvaluationContext context)
    {
        return new ReducedEvaluation(Build(reduced.InnerAlgebra, context));
    }

    private IEnumerableEvaluation BuildBindings(Bindings bindings)
    {
        return new BindingsEvaluation(bindings);
    }

    private IEnumerableEvaluation BuildGroupBy(GroupBy groupBy, PullEvaluationContext context)
    {
        var ret = new GroupByEvaluation(groupBy, Build(groupBy.InnerAlgebra, context));
        foreach (SparqlVariable? aggregate in groupBy.Aggregates)
        {
            if (aggregate != null)
            {
                ret.AddAggregateProvider(this.Build(aggregate, context));
            }
        }
        return ret;
    }

    private IEnumerableEvaluation BuildHaving(Having having, PullEvaluationContext context)
    {
        return new HavingEvaluation(having, Build(having.InnerAlgebra, context));
    }

    private Func<IAggregateEvaluation> Build(SparqlVariable aggregateVariable, PullEvaluationContext context)
    {
        if (!aggregateVariable.IsAggregate)
        {
            throw new ArgumentException("Provided variable is not an aggregate.", nameof(aggregateVariable));
        }

        return aggregateVariable.Aggregate switch
        {
            CountAggregate count => () => new CountAggregateEvaluation(count.Expression, count.Variable, aggregateVariable.Name, context),
            CountAllAggregate countAll => () => new CountAllAggregateEvaluation(countAll, aggregateVariable.Name),
            SumAggregate sum => () => new SumAggregateEvaluation(sum.Expression, sum.Distinct, aggregateVariable.Name, context),
            AverageAggregate avg => () => new AverageAggregateEvaluation(avg.Expression, avg.Distinct, aggregateVariable.Name, context),
            MaxAggregate max => () => new MaxAggregateEvaluation(max.Expression, max.Variable, aggregateVariable.Name, context),
            MinAggregate min => () => new MinAggregateEvaluation(min.Expression, min.Variable, aggregateVariable.Name, context),
            SampleAggregate sample => () => new SampleAggregateEvaluation(sample.Expression, aggregateVariable.Name, context),
            GroupConcatAggregate groupConcat => () => new GroupConcatAggregateEvaluation(groupConcat.Expression, groupConcat.SeparatorExpression, aggregateVariable.Name, context),
            _ => throw new RdfQueryException($"Unsupported aggregate {aggregateVariable.Aggregate}")
        };
    }

    private IEnumerableEvaluation BuildSubQuery(SubQuery subQuery, PullEvaluationContext context)
    {
        var autoVarPrefix = "_" + context.AutoVarFactory.Prefix;
        while (subQuery.Variables.Any(v => v.StartsWith(autoVarPrefix)))
        {
            autoVarPrefix = "_" + autoVarPrefix;
        }
        PullEvaluationContext subContext = context.MakeSubContext(subQuery.Query.DefaultGraphNames, subQuery.Query.NamedGraphNames);
        ISparqlAlgebra? queryAlgebra = subQuery.Query.ToAlgebra(true, new[] { new PushDownAggregatesOptimiser(autoVarPrefix) });
        return new SubQueryEvaluation(subQuery,Build(queryAlgebra, subContext), subContext);
    }

    private IEnumerableEvaluation BuildSubQueryPattern(SubQueryPattern subQueryPattern, PullEvaluationContext context)
    {
        var autoVarPrefix = "_" + context.AutoVarFactory.Prefix;
        while (subQueryPattern.Variables.Any(v => v.StartsWith(autoVarPrefix)))
        {
            autoVarPrefix = "_" + autoVarPrefix;
        }
        PullEvaluationContext subContext = context.MakeSubContext(subQueryPattern.SubQuery.DefaultGraphNames, subQueryPattern.SubQuery.NamedGraphNames);
        ISparqlAlgebra? queryAlgebra = subQueryPattern.SubQuery.ToAlgebra(true, new[] { new PushDownAggregatesOptimiser(autoVarPrefix) });
        return new SubQueryEvaluation( subQueryPattern,Build(queryAlgebra, subContext), subContext);
    }

    private IEnumerableEvaluation BuildPropertyPathPattern(PropertyPathPattern ppPattern, PullEvaluationContext context)
    {
        return new PropertyPathPatternEvaluation(Build(ppPattern.Path, ppPattern.Subject, ppPattern.Object, context), ppPattern.Subject, ppPattern.Object);
    }

    private IPathEvaluation Build(ISparqlPath path, PatternItem pathStart, PatternItem pathEnd,
        PullEvaluationContext context)
    {
        return path switch
        {
            SequencePath sequencePath => BuildSequencePath(sequencePath, pathStart, pathEnd, context),
            Property propertyPath => new PropertyPathEvaluation(propertyPath.Predicate, pathStart, pathEnd),
            InversePath inversePath => new InversePathEvaluation(Build(inversePath.Path, pathStart, pathEnd, context), pathStart, pathEnd),
            AlternativePath altPath => new PathUnionEvaluation(Build(altPath.LhsPath, pathStart, pathEnd, context),
                Build(altPath.RhsPath, pathStart, pathEnd, context)),
            ZeroOrOne zeroOrOne => new RepeatablePathEvaluation(0, 1, Build(zeroOrOne.Path, pathStart, pathEnd, context), pathEnd),
            ZeroOrMore zeroOrMore => new RepeatablePathEvaluation(0, -1, Build(zeroOrMore.Path, pathStart, pathEnd, context), pathEnd),
            OneOrMore oneOrMore => new RepeatablePathEvaluation(1, -1, Build(oneOrMore.Path, pathStart, pathEnd, context), pathEnd),
            NegatedSet negatedSet => new NegatedSetPathEvaluation(negatedSet, pathStart, pathEnd),
            _ => throw new RdfQueryException($"Unsupported query algebra {path} ({path.GetType()})")
        };
    }

    private IPathEvaluation BuildSequencePath(SequencePath sequencePath,PatternItem pathStart, PatternItem pathEnd,
        PullEvaluationContext context)
    {
        var joinVar = context.AutoVarFactory.NextId();
        var joinVarPattern = new VariablePattern(joinVar);
        return new SequencePathEvaluation(
            Build(sequencePath.LhsPath, pathStart, joinVarPattern, context),
            Build(sequencePath.RhsPath, joinVarPattern, pathEnd, context),
            joinVar);
    }

    private IEnumerableEvaluation BuildMinus(Minus minus, PullEvaluationContext context)
    {
        IEnumerableEvaluation lhsEval = Build(minus.Lhs, context);
        IEnumerableEvaluation rhsEval = Build(minus.Rhs, context);
        return new MinusEvaluation(minus, lhsEval, rhsEval);
    }
}

internal class IdentityEvaluation : IEnumerableEvaluation
{
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        yield return input ?? new Set();
    }
}
