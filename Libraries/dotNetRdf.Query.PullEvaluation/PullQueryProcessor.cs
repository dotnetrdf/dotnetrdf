using System.Collections;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace dotNetRdf.Query.PullEvaluation;

public class PullQueryProcessor : IAsyncEnumerable<ISet>
{
    private readonly ISparqlAlgebra _algebra;
    private readonly ISparqlDataset _dataset;
    
    public PullQueryProcessor(ISparqlAlgebra algebra, ISparqlDataset dataset)
    {
        _algebra = algebra;
        _dataset = dataset;
    }

    public IAsyncEnumerator<ISet> GetAsyncEnumerator(CancellationToken cancellationToken)
    {
        var builder = new EvaluationBuilder();
        var context = new PullEvaluationContext(_dataset);
        return builder.Build(_algebra, context);
    }
}