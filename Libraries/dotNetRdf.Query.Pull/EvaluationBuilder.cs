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
    public IAsyncEvaluation Build(ISparqlAlgebra algebra, PullEvaluationContext context)
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

    private IAsyncEvaluation BuildLeftJoin(ILeftJoin leftJoin, PullEvaluationContext context)
    {
        IAsyncEvaluation lhs = Build(leftJoin.Lhs, context);
        IAsyncEvaluation rhs = Build(leftJoin.Rhs, context);
        var joinVars = new HashSet<string>(leftJoin.Lhs.Variables);
        joinVars.IntersectWith(new HashSet<string>(leftJoin.Rhs.Variables));
        if (joinVars.Count == 0)
        {
            if (leftJoin.Filter == null)
            {
                return new AsyncCrossProductJoinEvaluation(lhs, rhs);
            }
            else
            {
                return new AsyncCrossProductLeftJoinEvaluation(lhs, rhs, leftJoin.Filter);
            }
        }
        var joinEval = new AsyncLeftJoinEvaluation(
            lhs,
            rhs,
            joinVars.ToArray(),
            leftJoin.Rhs.Variables.ToArray(),
            leftJoin.Filter
            );
        return joinEval;
    }
    
    private IAsyncEvaluation BuildTriplePattern(ITriplePattern triplePattern, PullEvaluationContext context)
    {
        if (triplePattern is IMatchTriplePattern matchTriplePattern)
        {
            return new AsyncTriplePatternEvaluation(matchTriplePattern);
        }

        if (triplePattern is BindPattern bindPattern)
        {
            if (bindPattern.InnerExpression.Variables.Any())
            {
                throw new RdfQueryException("Cannot process BIND with variables as first triple pattern");
            }

            return new AsyncBindPatternEvaluation(bindPattern, null);
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

    private IAsyncEvaluation BuildJoin(Join join, PullEvaluationContext context)
    {
        ISet<string> joinVars = new HashSet<string>(join.Lhs.Variables);
        joinVars.IntersectWith(join.Rhs.Variables);
        IAsyncEvaluation lhs = Build(join.Lhs, context);
        IAsyncEvaluation rhs = Build(join.Rhs, context);
        return joinVars.Count > 0 ? new AsyncJoinEvaluation(lhs, rhs, joinVars.ToArray()) : new AsyncCrossProductJoinEvaluation(lhs, rhs);
    }

    private IAsyncEvaluation BuildBgp(IBgp bgp, PullEvaluationContext context)
    {
        if (bgp.TriplePatterns.Count == 0)
        {
            return new IdentityEvaluation();
        }
        ISet<string> boundVars = new HashSet<string>(bgp.TriplePatterns[0].Variables);
        IAsyncEvaluation result = BuildTriplePattern(bgp.TriplePatterns[0], context);
        for (var i = 1; i < bgp.TriplePatterns.Count; i++)
        {
            ITriplePattern tp = bgp.TriplePatterns[i];
            ISet<string> joinVars = new HashSet<string>(boundVars);
            joinVars.IntersectWith(tp.Variables);
            boundVars.UnionWith(tp.Variables);
            result = tp switch
            {
                FilterPattern fp => new AsyncSparqlFilterEvaluation(fp.Filter, result, true),
                BindPattern bp => new AsyncBindPatternEvaluation(bp, result),
                _ => joinVars.Count > 0
                    ? new AsyncJoinEvaluation(result, BuildTriplePattern(tp, context), joinVars.ToArray()) 
                    : new AsyncCrossProductJoinEvaluation(result, BuildTriplePattern(tp, context))
            };
        }
        return result;
    }

    private IAsyncEvaluation BuildSelect(Select select, PullEvaluationContext context)
    {
        if (select.IsSelectAll) { return Build(select.InnerAlgebra, context); }

        return new AsyncSelectEvaluation(select, Build(select.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildFilter(Filter filter, PullEvaluationContext context)
    {
        return new AsyncFilterEvaluation(filter, Build(filter.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildUnion(Union union, PullEvaluationContext context)
    {
        return new AsyncUnionEvaluation(Build(union.Lhs, context), Build(union.Rhs, context));
    }

    private IAsyncEvaluation BuildGraph(Query.Algebra.Graph graph, PullEvaluationContext context)
    {
        if (graph.GraphSpecifier.TokenType == Token.VARIABLE)
        {
            return new AsyncGraphEvaluation(graph.GraphSpecifier.Value.TrimStart('?'), Build(graph.InnerAlgebra, context));
        }

        if (graph.GraphSpecifier.TokenType == Token.URI)
        {
            // TODO: Use URI factory and node factory from context
            var graphName = new UriNode(new Uri(graph.GraphSpecifier.Value));
            return new AsyncGraphEvaluation(graphName, Build(graph.InnerAlgebra, context));
        }
        throw new RdfQueryException($"Unsupported graph specifier token {graph.GraphSpecifier}");
    }

    private IAsyncEvaluation BuildAsk(Ask ask, PullEvaluationContext context)
    {
        return new AsyncAskEvaluation(Build(ask.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildAskAnyTriples()
    {
        return new AsyncAskAnyTriplesEvaluation();
    }

    private IAsyncEvaluation BuildDistinct(Distinct distinct, PullEvaluationContext context)
    {
        return new AsyncDistinctEvaluation(Build(distinct.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildOrderBy(OrderBy orderBy, PullEvaluationContext context)
    {
        return new AsyncOrderByEvaluation(orderBy, Build(orderBy.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildSlice(Slice slice, PullEvaluationContext context)
    {
        if (slice.Limit > 0)
        {
            context.SliceLength = slice.Limit;
            if (slice.Offset > 0) context.SliceLength += slice.Offset;
        }

        return new AsyncSliceEvaluation(slice, Build(slice.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildExtend(Extend extend, PullEvaluationContext context)
    {
        return new AsyncExtendEvaluation(extend, context, Build(extend.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildReduced(Reduced reduced, PullEvaluationContext context)
    {
        return new AsyncReducedEvaluation(Build(reduced.InnerAlgebra, context));
    }

    private IAsyncEvaluation BuildBindings(Bindings bindings)
    {
        return new AsyncBindingsEvaluation(bindings);
    }

    private IAsyncEvaluation BuildGroupBy(GroupBy groupBy, PullEvaluationContext context)
    {
        var ret = new AsyncGroupByEvaluation(groupBy, Build(groupBy.InnerAlgebra, context));
        foreach (SparqlVariable? aggregate in groupBy.Aggregates)
        {
            if (aggregate != null)
            {
                ret.AddAggregateProvider(this.Build(aggregate, context));
            }
        }
        return ret;
    }

    private IAsyncEvaluation BuildHaving(Having having, PullEvaluationContext context)
    {
        return new AsyncHavingEvaluation(having, Build(having.InnerAlgebra, context));
    }

    private Func<IAsyncAggregation> Build(SparqlVariable aggregateVariable, PullEvaluationContext context)
    {
        if (!aggregateVariable.IsAggregate)
        {
            throw new ArgumentException("Provided variable is not an aggregate.", nameof(aggregateVariable));
        }

        return aggregateVariable.Aggregate switch
        {
            CountAggregate count => () => new AsyncCountAggregate(count.Expression, count.Variable, aggregateVariable.Name, context),
            CountAllAggregate countAll => () => new AsyncCountAllAggregate(countAll, aggregateVariable.Name),
            SumAggregate sum => () => new AsyncSumAggregate(sum.Expression, sum.Distinct, aggregateVariable.Name, context),
            AverageAggregate avg => () => new AsyncAverageAggregate(avg.Expression, avg.Distinct, aggregateVariable.Name, context),
            MaxAggregate max => () => new AsyncMaxAggregate(max.Expression, max.Variable, aggregateVariable.Name, context),
            MinAggregate min => () => new AsyncMinAggregate(min.Expression, min.Variable, aggregateVariable.Name, context),
            SampleAggregate sample => () => new AsyncSampleAggregation(sample.Expression, aggregateVariable.Name, context),
            GroupConcatAggregate groupConcat => () => new AsyncGroupConcatAggregate(groupConcat.Expression, groupConcat.SeparatorExpression, aggregateVariable.Name, context),
            _ => throw new RdfQueryException($"Unsupported aggregate {aggregateVariable.Aggregate}")
        };
    }

    private IAsyncEvaluation BuildSubQuery(SubQuery subQuery, PullEvaluationContext context)
    {
        var autoVarPrefix = "_" + context.AutoVarFactory.Prefix;
        while (subQuery.Variables.Any(v => v.StartsWith(autoVarPrefix)))
        {
            autoVarPrefix = "_" + autoVarPrefix;
        }
        PullEvaluationContext subContext = context.MakeSubContext(subQuery.Query.DefaultGraphNames, subQuery.Query.NamedGraphNames);
        ISparqlAlgebra? queryAlgebra = subQuery.Query.ToAlgebra(true, [new PushDownAggregatesOptimiser(autoVarPrefix)]);
        return new AsyncSubQueryEvaluation(subQuery,Build(queryAlgebra, subContext), subContext);
    }

    private IAsyncEvaluation BuildSubQueryPattern(SubQueryPattern subQueryPattern, PullEvaluationContext context)
    {
        var autoVarPrefix = "_" + context.AutoVarFactory.Prefix;
        while (subQueryPattern.Variables.Any(v => v.StartsWith(autoVarPrefix)))
        {
            autoVarPrefix = "_" + autoVarPrefix;
        }
        PullEvaluationContext subContext = context.MakeSubContext(subQueryPattern.SubQuery.DefaultGraphNames, subQueryPattern.SubQuery.NamedGraphNames);
        ISparqlAlgebra? queryAlgebra = subQueryPattern.SubQuery.ToAlgebra(true, [new PushDownAggregatesOptimiser(autoVarPrefix)]);
        return new AsyncSubQueryEvaluation( subQueryPattern,Build(queryAlgebra, subContext), subContext);
    }

    private IAsyncEvaluation BuildPropertyPathPattern(PropertyPathPattern ppPattern, PullEvaluationContext context)
    {
        return new AsyncPropertyPathPatternEvaluation(Build(ppPattern.Path, ppPattern.Subject, ppPattern.Object, context), ppPattern.Subject, ppPattern.Object);
    }

    private IAsyncPathEvaluation Build(ISparqlPath path, PatternItem pathStart, PatternItem pathEnd,
        PullEvaluationContext context)
    {
        return path switch
        {
            SequencePath sequencePath => BuildSequencePath(sequencePath, pathStart, pathEnd, context),
            Property propertyPath => new AsyncPropertyPathEvaluation(propertyPath.Predicate, pathEnd),
            InversePath inversePath => new AsyncInversePathEvaluation(Build(inversePath.Path, pathStart, pathEnd, context), pathEnd),
            AlternativePath altPath => new AsyncPathUnionEvaluation(Build(altPath.LhsPath, pathStart, pathEnd, context),
                Build(altPath.RhsPath, pathStart, pathEnd, context)),
            ZeroOrOne zeroOrOne => new AsyncRepeatablePathEvaluation(0, 1, Build(zeroOrOne.Path, pathStart, pathEnd, context)),
            ZeroOrMore zeroOrMore => new AsyncRepeatablePathEvaluation(0, -1, Build(zeroOrMore.Path, pathStart, pathEnd, context)),
            OneOrMore oneOrMore => new AsyncRepeatablePathEvaluation(1, -1, Build(oneOrMore.Path, pathStart, pathEnd, context)),
            NegatedSet negatedSet => new AsyncNegatedSetPathEvaluation(negatedSet, pathEnd),
            _ => throw new RdfQueryException($"Unsupported query algebra {path} ({path.GetType()})")
        };
    }

    private IAsyncPathEvaluation BuildSequencePath(SequencePath sequencePath,PatternItem pathStart, PatternItem pathEnd,
        PullEvaluationContext context)
    {
        var joinVar = context.AutoVarFactory.NextId();
        var joinVarPattern = new VariablePattern(joinVar);
        return new AsyncSequencePathEvaluation(
            Build(sequencePath.LhsPath, pathStart, joinVarPattern, context),
            Build(sequencePath.RhsPath, joinVarPattern, pathEnd, context));
    }

    private IAsyncEvaluation BuildMinus(Minus minus, PullEvaluationContext context)
    {
        IAsyncEvaluation lhsEval = Build(minus.Lhs, context);
        IAsyncEvaluation rhsEval = Build(minus.Rhs, context);
        return new AsyncMinusEvaluation(minus, lhsEval, rhsEval);
    }
}

internal class IdentityEvaluation : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        yield return input ?? new Set();
    }
}
