using System.Text;
using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

public class AsyncGroupConcatAggregate : IAsyncAggregation
{
    private readonly StringBuilder _builder;
    private readonly ISparqlExpression _valueExpression;
    private readonly ISparqlExpression _separatorExpression;
    private readonly bool _hasSeparator;
    private readonly bool _separatorIsConstant;
    private readonly string? _separator;
    private readonly PullEvaluationContext _context;
    private bool _hasValue;

    public AsyncGroupConcatAggregate(
        ISparqlExpression valueExpression,
        ISparqlExpression? separatorExpression,
        string variableName,
        PullEvaluationContext context
    )
    {
        VariableName = variableName;
        _valueExpression = valueExpression;
        if (separatorExpression != null)
        {
            _hasSeparator = true;
            _separatorExpression = separatorExpression;
            if (_separatorExpression is ConstantTerm separatorConstant)
            {
                _separator = separatorConstant.Node.AsString();
                _separatorIsConstant = true;
            }
        }

        _builder = new StringBuilder();
        _context = context;
    }
    
    public string VariableName { get; }
    public INode? Value { get; private set; }
    public void Start()
    {
        _hasValue = false;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        try
        {
            IValuedNode value = _valueExpression.Accept(_context.ExpressionProcessor, _context, expressionContext);
            if (value == null)
            {
                return true;
            }
            if (_hasValue && _hasSeparator)
            {
                if (_separatorIsConstant)
                {
                    _builder.Append(_separator);
                }
                else
                {
                    IValuedNode separatorTerm = _separatorExpression.Accept(_context.ExpressionProcessor, _context,
                        expressionContext);
                    _builder.Append(separatorTerm.ToString());
                }
            }

            _builder.Append(value.AsString());
            _hasValue = true;
        }
        catch (RdfQueryException)
        {
            // Ignore expressions that fail to evaluate
        }

        return true;
    }

    public void End()
    {
        if (_hasValue)
        {
            Value = new StringNode(_builder.ToString());
        }
    }
}