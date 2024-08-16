using System.Runtime.CompilerServices;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

/// <summary>
/// Async evaluation of ASK
///
/// Yields a single set empty set if the inner algebra yields any result, otherwise yields nothing.
/// </summary>
internal class AsyncAskEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;

    internal AsyncAskEvaluation(IAsyncEvaluation inner)
    {
        _inner = inner;
    }

    /// <inheritdoc />
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await using IAsyncEnumerator<ISet> innerEnumerator = _inner
            .Evaluate(context, input, activeGraph, cancellationToken)
            .GetAsyncEnumerator(cancellationToken);
        var hasNext = await innerEnumerator.MoveNextAsync();
        if (hasNext) yield return new Set();
    }
}