using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

/// <summary>
/// Implements evaluation of a TriplePattern against a PullEvaluationContext returning all matching solution bindings.
/// </summary>
public class AsyncTriplePatternEvaluation : IAsyncEvaluation
{
    private readonly IMatchTriplePattern _triplePattern;

    /// <summary>
    /// Construct a new triple pattern evaluator
    /// </summary>
    /// <param name="triplePattern"></param>
    public AsyncTriplePatternEvaluation(IMatchTriplePattern triplePattern)
    {
        _triplePattern = triplePattern;
    }


    /// <summary>
    /// Evaluate the triple pattern optionally using the given input solution.
    /// </summary>
    /// <param name="context">Evaluation context</param>
    /// <param name="input">Optional input solution</param>
    /// <returns></returns>
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        return context.GetTriples(_triplePattern, input, activeGraph)
            .Select(t=>_triplePattern.Evaluate(context,t))
            .Where(set => set != null)
            .Select(set => input != null ? set.Join(input) : set)
            .ToAsyncEnumerable();
    }
}