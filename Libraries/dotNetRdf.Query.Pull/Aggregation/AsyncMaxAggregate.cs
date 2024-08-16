using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.Pull.Aggregation;

internal class AsyncMaxAggregate(
    ISparqlExpression valueExpression,
    string? maxVar,
    string variableName,
    PullEvaluationContext context)
    : IAsyncAggregation
{
    private INode? _max = null;
    private readonly ISparqlNodeComparer _comparer = context.OrderingComparer;

    public string VariableName { get; } = variableName;
    public INode? Value { get { return _max; } }
    private bool _failed = false;
    public void Start()
    {
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (_failed) return true;
        INode? tmp = maxVar != null
            ? (expressionContext.Bindings.ContainsVariable(maxVar) ? expressionContext.Bindings[maxVar] : null)
            : valueExpression.Accept(context.ExpressionProcessor, context, expressionContext);
        if (tmp == null)
        {
            return true;
        }

        if (_max == null)
        {
            _max = tmp;
            return true;
        }

        if (_comparer.TryCompare(tmp, _max, out var result))
        {
            if (result > 0)
            {
                _max = tmp;
            }
            return true;
        }

        _failed = true;
        _max = null;
        return true;
    }

    public void End()
    {
    }
}