using VDS.RDF;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.Pull.Aggregation;

internal class AsyncSampleAggregation(ISparqlExpression expression, string name, PullEvaluationContext context)
    : IAsyncAggregation
{
    public string VariableName { get; } = name;
    public INode? Value { get; private set; }
    public void Start()
    {
        
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (Value != null) return true;
        Value = expression.Accept(context.ExpressionProcessor, context, expressionContext);
        return true;
    }

    public void End()
    {
    }
}