using VDS.RDF;
using VDS.RDF.Query;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

public class AsyncMinAggregate : IAsyncAggregation
{
    private INode? _min = null;
    private ISparqlNodeComparer _comparer;
    private ISparqlExpression _expression;
    private string? _maxVar;
    private readonly PullEvaluationContext _context;
    
    public AsyncMinAggregate(ISparqlExpression valueExpression, string? maxVar, string variableName,
        PullEvaluationContext context)
    {
        _comparer = context.OrderingComparer;
        _expression = valueExpression;
        _maxVar = maxVar;
        _context = context;
        VariableName = variableName;
    }
    public string VariableName { get; }
    public INode? Value { get { return _min; } }
    public void Start()
    {
    }

    public bool Accept(ISet s)
    {
        INode? tmp = _maxVar != null
            ? (s.ContainsVariable(_maxVar) ? s[_maxVar] : null)
            : _expression.Accept(_context.ExpressionProcessor, _context, s);
        if (tmp == null)
        {
            return true;
        }

        if (_min == null)
        {
            _min = tmp;
            return true;
        }

        if (_comparer.TryCompare(tmp, _min, out var result) && result < 0)
        {
            _min = tmp;
            return true;
        }

        return true;
    }

    public void End()
    {
    }
}