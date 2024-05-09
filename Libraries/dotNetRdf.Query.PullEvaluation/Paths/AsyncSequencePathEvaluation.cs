using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal class AsyncSequencePathEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string joinVar)
    : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet lhsSolution in lhs.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (lhsSolution.ContainsVariable(joinVar) && lhsSolution[joinVar] != null)
            {
                await foreach (ISet rhsSolution in rhs.Evaluate(context, input?.Join(lhsSolution) ?? lhsSolution,
                                   activeGraph, cancellationToken))
                {
                    rhsSolution.Remove(joinVar);
                    yield return rhsSolution;
                }
            }
        }
    }
}