using System.Runtime.CompilerServices;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncHavingEvaluation(Having having, IAsyncEvaluation inner) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet solutionBinding in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            IValuedNode? result =
                having.HavingClause.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(solutionBinding, activeGraph));
            if (result?.AsBoolean() ?? false)
            {
                yield return solutionBinding;
            }
        }
    }
}