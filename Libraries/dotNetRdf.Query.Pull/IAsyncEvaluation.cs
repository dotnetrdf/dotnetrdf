using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull;

internal interface IAsyncEvaluation
{
    /// <summary>
    /// Run the evaluation with an optional set of input variable bindings.
    /// </summary>
    /// <param name="context">The evaluation context to use.</param>
    /// <param name="input">The variable bindings to apply to the evaluation. Null if there are no input variable bindings.</param>
    /// <param name="activeGraph">Overrides the active graph(s) in the context dataset.</param>
    /// <param name="cancellationToken">The cancellation token that can be used to cancel the evaluation.</param>
    /// <returns>An async enumerable over the results of the evaluation.</returns>
    IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default);
}