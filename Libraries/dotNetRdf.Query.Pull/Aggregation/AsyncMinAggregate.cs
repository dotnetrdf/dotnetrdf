using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.Pull.Aggregation;

internal class AsyncMinAggregate(
    ISparqlExpression valueExpression,
    string? maxVar,
    string variableName,
    PullEvaluationContext context)
    : IAsyncAggregation
{
    private INode? _min = null;
    private readonly ISparqlNodeComparer _comparer = context.OrderingComparer;

    public string VariableName { get; } = variableName;
    public INode? Value { get { return _min; } }

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
            _failed = true;
            return true;
        }

        if (_min == null)
        {
            _min = tmp;
            return true;
        }

        if (_comparer.TryCompare(tmp, _min, out var result))
        {
            if (result < 0)
            {
                _min = tmp;
            }
            return true;
        }

        _failed = true;
        _min = null;
        return true;
    }

    public void End()
    {
    }
}