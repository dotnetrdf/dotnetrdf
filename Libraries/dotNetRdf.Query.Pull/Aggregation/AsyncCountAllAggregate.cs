using VDS.RDF.Nodes;
using VDS.RDF.Query.Aggregates.Sparql;

namespace VDS.RDF.Query.Pull.Aggregation;

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