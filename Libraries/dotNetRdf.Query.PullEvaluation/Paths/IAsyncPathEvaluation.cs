using System.Data;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation.Paths;

internal interface IAsyncPathEvaluation
{
    public IAsyncEnumerable<PathResult> Evaluate(PatternItem stepStart, PullEvaluationContext context, ISet? input,
        IRefNode? activeGraph, CancellationToken cancellationToken);
}