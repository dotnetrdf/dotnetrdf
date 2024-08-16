using dotNetRdf.Query.Pull.Aggregation;
using dotNetRdf.Query.Pull.Algebra;
using dotNetRdf.Query.Pull.Paths;
using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Query.Patterns;
using Graph = VDS.RDF.Query.Algebra.Graph;

namespace dotNetRdf.Query.Pull;

internal class EvaluationBuilder
{
    public IAsyncEvaluation Build(ISparqlAlgebra algebra, PullEvaluationContext context)
    {
        return algebra switch
        {
            Filter filter => BuildFilter(filter, context),
            Graph graph => BuildGraph(graph, context),
            IBgp bgp => BuildBgp(bgp, context),
            Join join => BuildJoin(join, context),
            LeftJoin leftJoin => BuildLeftJoin(leftJoin, context),
            Select select => BuildSelect(select, context),
            Union union => BuildUnion(union, context),
            Ask ask => BuildAsk(ask, context),
            Distinct distinct => BuildDistinct(distinct, context),
            OrderBy orderBy => BuildOrderBy(orderBy, context),
            Slice slice => BuildSlice(slice, context),
            Extend extend => BuildExtend(extend, context),
            Reduced reduced => BuildReduced(reduced, context),
            Bindings bindings => BuildBindings(bindings, context),
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
        return new AsyncJoinEvaluation(Build(join.Lhs, context), Build(join.Rhs, context), joinVars.ToArray());
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
            joinVars.IntersectWith(bgp.Variables);
            boundVars.UnionWith(bgp.Variables);
            result = tp switch
            {
                FilterPattern fp => new AsyncSparqlFilterEvaluation(fp.Filter, result, true),
                BindPattern bp => new AsyncBindPatternEvaluation(bp, result),
                _ => new AsyncJoinEvaluation(result, BuildTriplePattern(tp, context), joinVars.ToArray())
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

    private IAsyncEvaluation BuildGraph(Graph graph, PullEvaluationContext context)
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

    private IAsyncEvaluation BuildBindings(Bindings bindings, PullEvaluationContext context)
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
        ISparqlAlgebra? queryAlgebra = subQuery.Query.ToAlgebra(true, new[] { new PushDownAggregatesOptimiser(autoVarPrefix) });
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
        ISparqlAlgebra? queryAlgebra = subQueryPattern.SubQuery.ToAlgebra(true, new[] { new PushDownAggregatesOptimiser(autoVarPrefix) });
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
            Property propertyPath => new AsyncPropertyPathEvaluation(propertyPath.Predicate, pathStart, pathEnd),
            InversePath inversePath => new AsyncInversePathEvaluation(Build(inversePath.Path, pathStart, pathEnd, context), pathStart, pathEnd),
            AlternativePath altPath => new AsyncPathUnionEvaluation(Build(altPath.LhsPath, pathStart, pathEnd, context),
                Build(altPath.RhsPath, pathStart, pathEnd, context)),
            ZeroOrOne zeroOrOne => new AsyncRepeatablePathEvaluation(0, 1, Build(zeroOrOne.Path, pathStart, pathEnd, context), pathEnd),
            ZeroOrMore zeroOrMore => new AsyncRepeatablePathEvaluation(0, -1, Build(zeroOrMore.Path, pathStart, pathEnd, context), pathEnd),
            OneOrMore oneOrMore => new AsyncRepeatablePathEvaluation(1, -1, Build(oneOrMore.Path, pathStart, pathEnd, context), pathEnd),
            NegatedSet negatedSet => new AsyncNegatedSetPathEvaluation(negatedSet, pathStart, pathEnd),
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
            Build(sequencePath.RhsPath, joinVarPattern, pathEnd, context),
            joinVar);
    }

    private IAsyncEvaluation BuildPropertyPath(INode predicate, PatternItem pathStart, PatternItem pathEnd,
        PullEvaluationContext _)
    {
        return new AsyncTriplePatternEvaluation(new TriplePattern(pathStart, new NodeMatchPattern(predicate), pathEnd));
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

/*
internal class PushDownAggregatesTransformer : IExpressionTransformer
{
    private readonly List<SparqlVariable> _havingVars = new();
    private readonly string _autoPrefix = "_" + Guid.NewGuid().ToString("N");
    private int _autoId = 0;
    
    public List<SparqlVariable> HavingVars { get { return _havingVars; } }
    public Having Transform(Having having)
    {
        if (having.InnerAlgebra is GroupBy groupBy)
        {
            ISparqlExpression havingExpression = having.HavingClause.Expression.Transform(this);
            var havingPattern = new GroupBy(groupBy.InnerAlgebra, groupBy.Grouping, groupBy.Aggregates.Union(this._havingVars));
            return new Having(havingPattern, new UnaryExpressionFilter(havingExpression));
        }

        return having;
    }

    public ISparqlExpression Transform(ISparqlExpression expr)
    {
        if (expr is AggregateTerm aggregateTerm)
        {
            var sv = new SparqlVariable(NextId(), aggregateTerm.Aggregate);
            this._havingVars.Add(sv);
            return new VariableTerm(sv.Name);
        }
        return expr.Transform(this);
    }

    private string NextId()
    {
        return _autoPrefix + _autoId++;
    }
}
*/