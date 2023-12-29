using System.Collections;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Datasets;

namespace dotNetRdf.Query.PullEvaluation;

public class PullQueryProcessor
{
    private readonly ISparqlAlgebra _algebra;
    private readonly ISparqlDataset _dataset;
    
    public PullQueryProcessor(ISparqlAlgebra algebra, ISparqlDataset dataset)
    {
        _algebra = algebra;
        _dataset = dataset;
    }

    public IAsyncEnumerable<ISet> Evaluate(CancellationToken cancellationToken = default)
    {
        var builder = new EvaluationBuilder();
        var context = new PullEvaluationContext(_dataset);
        IAsyncEvaluation evaluation = builder.Build(_algebra, context);
        return evaluation.Evaluate(context, null, cancellationToken);
    }
}