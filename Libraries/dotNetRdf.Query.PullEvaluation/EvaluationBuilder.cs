using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class EvaluationBuilder
{
    public IAsyncEvaluation Build(ISparqlAlgebra algebra, PullEvaluationContext context) 
    {
        if (algebra is IBgp bgp)
        {
            return Build(bgp, context);
        }

        throw new RdfQueryException($"Unsupported algebra {algebra}");
    }

    private IAsyncEvaluation Build(ITriplePattern triplePattern, PullEvaluationContext context)
    {
        if (triplePattern is IMatchTriplePattern matchTriplePattern)
        {
            return new AsyncTriplePatternEvaluation(matchTriplePattern);
        }

        throw new RdfQueryException($"Unsupported algebra {triplePattern}");
    }

    private IAsyncEvaluation Build(IBgp bgp, PullEvaluationContext context)
    {
        ISet<string> boundVars = new HashSet<string>(bgp.TriplePatterns[0].Variables);
        IAsyncEvaluation result = Build(bgp.TriplePatterns[0], context);
        for (var i = 1; i < bgp.TriplePatterns.Count; i++)
        {
            ITriplePattern tp = bgp.TriplePatterns[i];
            ISet<string> joinVars = new HashSet<string>(boundVars);
            joinVars.IntersectWith(bgp.Variables);
            result = new AsyncJoinEvaluation(result, Build(tp, context), joinVars.ToArray());
        }
        return result;
    }
}