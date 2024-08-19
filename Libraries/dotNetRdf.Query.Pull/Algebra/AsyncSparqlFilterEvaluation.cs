using System.Runtime.CompilerServices;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace VDS.RDF.Query.Pull.Algebra;

internal class AsyncSparqlFilterEvaluation(ISparqlFilter filter, IAsyncEvaluation inner, bool failSilently)
    : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool filterResult;
            try
            {
                filterResult = filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(innerResult, activeGraph))
                    .AsSafeBoolean();
            }
            catch (RdfQueryException)
            {
                // If the result processed was an identity result, swallow the error
                filterResult = false;
                if (!failSilently && innerResult.Variables.Any()) { throw; }
            }

            if (filterResult) yield return innerResult;
        }
    }
}