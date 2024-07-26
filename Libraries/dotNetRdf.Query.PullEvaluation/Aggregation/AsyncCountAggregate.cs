using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Expressions;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

internal class AsyncCountAggregate(
    ISparqlExpression valueExpression,
    string? countVar,
    string variableName,
    PullEvaluationContext context)
    : IAsyncAggregation
{
    private long _count = 0;

    public string VariableName { get; } = variableName;

    public INode? Value { get { return new LongNode(_count); } }

    public void Start()
    {
        _count = 0;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        if (countVar != null && expressionContext.Bindings.ContainsVariable(countVar) &&
            expressionContext.Bindings[countVar] != null)
        {
            _count++;
        }
        else
        {
            INode? tmp = valueExpression.Accept(context.ExpressionProcessor, context, expressionContext);
            if (tmp != null) _count++;
        }

        return true;
    }

    public void End()
    {

    }
}