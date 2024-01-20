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
                filterResult = _filter.SparqlFilter.Expression.Accept(context.ExpressionProcessor, context, innerResult)
                    .AsSafeBoolean();
            }
            catch (RdfQueryException)
            {
                // If the result processed was an identity result, swallow the error
                filterResult = false;
                if (innerResult.Variables.Any()) { throw; }
            }

            if (filterResult) yield return innerResult;
        }
    }
}