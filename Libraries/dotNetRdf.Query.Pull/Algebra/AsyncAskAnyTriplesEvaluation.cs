using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncAskAnyTriplesEvaluation : IAsyncEvaluation
{
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        if (context.HasAnyTriples(activeGraph))
        {
            return (input ?? new Set()).AsEnumerable().ToAsyncEnumerable();
        }
        return AsyncEnumerable.Empty<ISet>();
    }
}