using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncFilterEvaluation : IAsyncEvaluation
{
    private readonly Filter _filter;
    private readonly IAsyncEvaluation _inner;
    public AsyncFilterEvaluation(Filter filter, IAsyncEvaluation inner)
    {
        _filter = filter;
        _inner = inner;
    } 
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool filterResult;
            try
            {
                filterResult = _filter.SparqlFilter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(innerResult, activeGraph))
                    .AsSafeBoolean();
            }
            catch (RdfQueryException)
            {
                // If the result processed was an identity result, swallow the error
                filterResult = false;
                // If the result processed was an identity result, swallow the error
                // if (innerResult.Variables.Any()) { throw; } // Rethrowing the exception here causes DAWG tests dawg-bev-5 and dawg-bev-6 to fail
            }

            if (filterResult) yield return innerResult;
        }
    }
}