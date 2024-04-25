using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Filters;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncSparqlFilterEvaluation : IAsyncEvaluation
{
    private readonly ISparqlFilter _filter;
    private readonly IAsyncEvaluation _inner;
    private readonly bool _failSilently;

    public AsyncSparqlFilterEvaluation(ISparqlFilter filter, IAsyncEvaluation inner, bool failSilently)
    {
        _filter = filter;
        _inner = inner;
        _failSilently = failSilently;
    }
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode activeGraph, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (ISet innerResult in _inner.Evaluate(context, input, activeGraph, cancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
            bool filterResult;
            try
            {
                filterResult = _filter.Expression.Accept(context.ExpressionProcessor, context, new ExpressionContext(innerResult, activeGraph))
                    .AsSafeBoolean();
            }
            catch (RdfQueryException)
            {
                // If the result processed was an identity result, swallow the error
                filterResult = false;
                if (!_failSilently && innerResult.Variables.Any()) { throw; }
            }

            if (filterResult) yield return innerResult;
        }
    }
}