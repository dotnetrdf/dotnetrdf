using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.Pull.Algebra;

internal class AsyncGraphEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _inner;
    private readonly string? _graphVarName;
    private readonly IRefNode? _graphName;

    public AsyncGraphEvaluation(IRefNode graphName, IAsyncEvaluation inner)
    {
        _graphName = graphName;
        _inner = inner;
    }

    public AsyncGraphEvaluation(string variableName, IAsyncEvaluation inner)
    {
        _graphVarName = variableName;
        _inner = inner;
    }
    public IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default)
    {
        if (_graphName != null)
        {
            // Fixed graph filter
            return _inner.Evaluate(context, input, _graphName, cancellationToken);
        }
        if (input != null && input.ContainsVariable(_graphVarName) && input[_graphVarName] is IRefNode graphName)
        {
            // Graph variable is bound by input
            return _inner.Evaluate(context, input, graphName, cancellationToken);
        }

        return context.NamedGraphNames.Select(gn =>
            {
                ISet inputWithGraphBinding = input?.Copy() ?? new Set();
                if (_graphVarName != null)
                {
                    inputWithGraphBinding.Add(_graphVarName, gn);
                }

                return _inner.Evaluate(context, inputWithGraphBinding, gn, cancellationToken);
            })
            .Merge();
    }
}