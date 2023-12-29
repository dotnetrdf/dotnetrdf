using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

/// <summary>
/// Implements evaluation of a TriplePattern against a PullEvaluationContext returning all matching solution bindings.
/// </summary>
public class AsyncTriplePatternEvaluation : IAsyncEvaluation
{
    private readonly TriplePattern _triplePattern;
    private readonly PullEvaluationContext _evaluationContext;

    /// <summary>
    /// Construct a new triple pattern evaluator
    /// </summary>
    /// <param name="triplePattern"></param>
    /// <param name="evaluationContext"></param>
    public AsyncTriplePatternEvaluation(TriplePattern triplePattern, PullEvaluationContext evaluationContext)
    {
        _triplePattern = triplePattern;
        _evaluationContext = evaluationContext;
    }


    /// <summary>
    /// Evaluate the triple pattern optionally using the given input solution.
    /// </summary>
    /// <param name="input"></param>
    /// <returns></returns>
    public IAsyncEnumerable<ISet> Evaluate(ISet? input)
    {
        return _evaluationContext.GetTriples(_triplePattern, input)
            .Select(t=>_triplePattern.Evaluate(_evaluationContext,t))
            .Where(set => set != null)
            .ToAsyncEnumerable();
    }
}