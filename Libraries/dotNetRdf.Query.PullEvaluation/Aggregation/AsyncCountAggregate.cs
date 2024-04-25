using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

public class AsyncCountAggregate : IAsyncAggregation
{
    private readonly ISparqlExpression _expression;
    private long _count;
    private PullEvaluationContext _context;
    private string? _countVar;

    public AsyncCountAggregate(ISparqlExpression valueExpression, string? countVar, string variableName,
        PullEvaluationContext context)
    {
        _expression = valueExpression;
        VariableName = variableName;
        _context = context;
        _count = 0;
        _countVar = countVar;
    }

    public string VariableName { get; }

    public INode? Value { get { return new LongNode(_count); } }

    public void Start()
    {
        _count = 0;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (_countVar != null && expressionContext.Bindings.ContainsVariable(_countVar) &&
            expressionContext.Bindings[_countVar] != null)
        {
            _count++;
        }
        else
        {
            INode? tmp = _expression.Accept(_context.ExpressionProcessor, _context, expressionContext);
            if (tmp != null) _count++;
        }

        return true;
    }

    public void End()
    {

    }
}