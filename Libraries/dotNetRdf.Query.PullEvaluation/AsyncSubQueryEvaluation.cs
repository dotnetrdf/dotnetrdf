using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncSubQueryEvaluation: IAsyncEvaluation
{
    private readonly List<string> _subQueryVariables;
    private readonly IAsyncEvaluation _inner;
    private readonly PullEvaluationContext _innerContext;

    public AsyncSubQueryEvaluation(SubQuery subQuery, IAsyncEvaluation inner, PullEvaluationContext subContext)
    {
        _subQueryVariables = subQuery.Variables.ToList();
        _inner = inner;
        _innerContext = subContext;
    }

    public AsyncSubQueryEvaluation(SubQueryPattern subQueryPattern, IAsyncEvaluation inner,
        PullEvaluationContext subContext)
    {
        _subQueryVariables = subQueryPattern.SubQuery.Variables.Where(v => v.IsResultVariable).Select(v => v.Name)
            .ToList();
        _inner = inner;
        _innerContext = subContext;
    }

    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        CancellationToken cancellationToken = default)
    {
        ISet innerInput = new Set();
        if (input != null)
        {
            foreach (var v in _subQueryVariables)
            {
                if (input.ContainsVariable(v))
                {
                    innerInput.Add(v, input[v]);
                }
            }
        }

        return _inner.Evaluate(_innerContext, innerInput, activeGraph, cancellationToken);
    }
}