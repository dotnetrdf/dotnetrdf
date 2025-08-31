using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull;

public interface IEnumerableEvaluation
{
    public IEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph, CancellationToken cancellationToken = default);
}