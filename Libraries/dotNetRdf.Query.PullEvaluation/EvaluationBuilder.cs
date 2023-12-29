using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class EvaluationBuilder
{
    public IAsyncEnumerator<ISet> Build(ISparqlAlgebra algebra, PullEvaluationContext context) 
    {
        if (algebra is IBgp bgp)
        {
            return Build(bgp, context);
        }

        throw new RdfQueryException($"Unsupported algebra {algebra}");
    }

    private IAsyncEnumerator<ISet> Build(ITriplePattern triplePattern, PullEvaluationContext context)
    {
        if (triplePattern is IMatchTriplePattern matchTriplePattern)
        {
            return new TriplePatternEnumerator(matchTriplePattern, context);
        }

        throw new RdfQueryException($"Unsupported algebra {triplePattern}");
    }

    private IAsyncEnumerator<ISet> Build(IBgp bgp, PullEvaluationContext context)
    {
        ISet<string> boundVars = new HashSet<string>(bgp.TriplePatterns[0].Variables);
        IAsyncEnumerator<ISet> result = Build(bgp.TriplePatterns[0], context);
        for (var i = 1; i < bgp.TriplePatterns.Count; i++)
        {
            ITriplePattern tp = bgp.TriplePatterns[i];
            ISet<string> joinVars = new HashSet<string>(boundVars);
            joinVars.IntersectWith(bgp.Variables);
            result = new JoinEnumerator(result, Build(tp, context), joinVars.ToArray());
        }

        return result;
    }
}