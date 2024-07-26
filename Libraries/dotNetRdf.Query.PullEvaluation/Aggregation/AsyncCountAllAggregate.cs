using VDS.RDF;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregates.Sparql;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation.Aggregation;

public class AsyncCountAllAggregate : IAsyncAggregation
{
    private long _count = 0;
    public AsyncCountAllAggregate(CountAllAggregate ca, string variableName)
    {
        VariableName = variableName;
    }
    public string VariableName { get; }
    public INode? Value { get { return new LongNode(_count); } }
    public void Start()
    {
        _count = 0;
    }

    public bool Accept(ExpressionContext expressionContext)
    {
        _count++;
        return true;
    }

    public void End()
    {
    }
}