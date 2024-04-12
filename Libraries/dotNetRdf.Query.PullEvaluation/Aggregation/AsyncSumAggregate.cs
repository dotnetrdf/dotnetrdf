using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

public class AsyncSumAggregate : IAsyncAggregation
{
    private readonly ISparqlExpression _expression;

    private bool _error;
    private long _longSum = 0;
    private decimal _decimalSum = 0.0m;
    private float _floatSum = 0.0f;
    private double _doubleSum = 0.0d;
    private readonly PullEvaluationContext _context;
    private SparqlNumericType _maxtype = SparqlNumericType.NaN;
    private SparqlNumericType _numtype = SparqlNumericType.NaN;
    private readonly bool _distinct;
    private readonly HashSet<INode> _values;
    
    internal AsyncSumAggregate(ISparqlExpression valueExpression, bool distinct, string varName, PullEvaluationContext context)
    {
        VariableName = varName;
        _expression = valueExpression;
        _context = context;
        if (distinct) _values = new HashSet<INode>();
        _distinct = distinct;
    }
    public string VariableName { get; }

    public INode? Value
    {
        get
        {
            return _numtype switch
            {
                SparqlNumericType.NaN => null,
                SparqlNumericType.Integer => new LongNode(_longSum),
                SparqlNumericType.Decimal => new DecimalNode(_decimalSum),
                SparqlNumericType.Float => new FloatNode(_floatSum),
                SparqlNumericType.Double => new DoubleNode(_doubleSum),
                _ => null
            };
        }
    }

    public virtual void Start()
    {
        _longSum = 0;
        _decimalSum = 0.0m;
        _floatSum = 0.0f;
        _doubleSum = 0.0d;
    }

    public bool Accept(ISet s)
    {
        if (_error) return false;
        IValuedNode? tmp = _expression.Accept(_context.ExpressionProcessor, _context, s);
        if (tmp == null) { 
            _error = true;
            return false;
        }

        if (_distinct)
        {
            if (!_values.Add(tmp))
            {
                return true;
            }
        }
        _numtype = tmp.NumericType;
        if (_numtype == SparqlNumericType.NaN)
        {
            _error = true;
            return false;
        }
        
        // Track the Numeric Type
        if ((int)_numtype > (int)_maxtype)
        {
            _maxtype = _numtype;
        }

        // Increment the Totals based on the current Numeric Type
        switch (_maxtype)
        {
            case SparqlNumericType.Integer:
                _longSum += tmp.AsInteger();
                _decimalSum += tmp.AsDecimal();
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Decimal:
                _decimalSum += tmp.AsDecimal();
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Float:
                _floatSum += tmp.AsFloat();
                _doubleSum += tmp.AsDouble();
                break;
            case SparqlNumericType.Double:
                _doubleSum += tmp.AsDouble();
                break;
        }

        return true;
    }

    public void End()
    {
    }
}