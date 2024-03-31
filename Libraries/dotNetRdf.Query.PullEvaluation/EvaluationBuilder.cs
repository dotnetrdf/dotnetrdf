using VDS.RDF;
using VDS.RDF.Parsing.Tokens;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;
using Graph = VDS.RDF.Query.Algebra.Graph;

namespace dotNetRdf.Query.PullEvaluation;

public class EvaluationBuilder
{
    public IAsyncEvaluation Build(ISparqlAlgebra algebra)
    {
        return algebra switch
        {
            Filter filter => BuildFilter(filter),
            Graph graph => BuildGraph(graph),
            IBgp bgp => BuildBgp(bgp),
            Join join => BuildJoin(join),
            LeftJoin leftJoin => BuildLeftJoin(leftJoin),
            Select select => BuildSelect(select),
            Union union => BuildUnion(union),
            Ask ask => BuildAsk(ask),
            _ => throw new RdfQueryException($"Unsupported algebra {algebra}")
        };
    }

    private IAsyncEvaluation BuildLeftJoin(ILeftJoin leftJoin)
    {
        IAsyncEvaluation lhs = Build(leftJoin.Lhs);
        IAsyncEvaluation rhs = Build(leftJoin.Rhs);
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
    
    private IAsyncEvaluation BuildTriplePattern(ITriplePattern triplePattern)
    {
        if (triplePattern is IMatchTriplePattern matchTriplePattern)
        {
            return new AsyncTriplePatternEvaluation(matchTriplePattern);
        }

        throw new RdfQueryException($"Unsupported algebra {triplePattern}");
    }

    private IAsyncEvaluation BuildJoin(Join join)
    {
        ISet<string> joinVars = new HashSet<string>(join.Lhs.Variables);
        joinVars.IntersectWith(join.Rhs.Variables);
        return new AsyncJoinEvaluation(Build(join.Lhs), Build(join.Rhs), joinVars.ToArray());
    }

    private IAsyncEvaluation BuildBgp(IBgp bgp)
    {
        if (bgp.TriplePatterns.Count == 0)
        {
            return new IdentityEvaluation();
        }
        ISet<string> boundVars = new HashSet<string>(bgp.TriplePatterns[0].Variables);
        IAsyncEvaluation result = BuildTriplePattern(bgp.TriplePatterns[0]);
        for (var i = 1; i < bgp.TriplePatterns.Count; i++)
        {
            ITriplePattern tp = bgp.TriplePatterns[i];
            ISet<string> joinVars = new HashSet<string>(boundVars);
            joinVars.IntersectWith(bgp.Variables);
            boundVars.UnionWith(bgp.Variables);
            result = new AsyncJoinEvaluation(result, BuildTriplePattern(tp), joinVars.ToArray());
        }
        return result;
    }

    private IAsyncEvaluation BuildSelect(Select select)
    {
        if (select.IsSelectAll) { return Build(select.InnerAlgebra); }

        return new AsyncSelectEvaluation(select, Build(select.InnerAlgebra));
    }

    private IAsyncEvaluation BuildFilter(Filter filter)
    {
        return new AsyncFilterEvaluation(filter, Build(filter.InnerAlgebra));
    }

    private IAsyncEvaluation BuildUnion(Union union)
    {
        return new AsyncUnionEvaluation(Build(union.Lhs), Build(union.Rhs));
    }

    private IAsyncEvaluation BuildGraph(Graph graph)
    {
        if (graph.GraphSpecifier.TokenType == Token.VARIABLE)
        {
            return new AsyncGraphEvaluation(graph.GraphSpecifier.Value.TrimStart('?'), Build(graph.InnerAlgebra));
        }

        if (graph.GraphSpecifier.TokenType == Token.URI)
        {
            // TODO: Use URI factory and node factory from context
            var graphName = new UriNode(new Uri(graph.GraphSpecifier.Value));
            return new AsyncGraphEvaluation(graphName, Build(graph.InnerAlgebra));
        }
        throw new RdfQueryException($"Unsupported graph specifier token {graph.GraphSpecifier}");
    }

    private IAsyncEvaluation BuildAsk(Ask ask)
    {
        return new AsyncAskEvaluation(Build(ask.InnerAlgebra));
    }
}

internal class IdentityEvaluation : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        yield return input ?? new Set();
    }
}