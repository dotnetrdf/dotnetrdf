using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncBindingsEvaluation(Bindings bindings) : IAsyncEvaluation
{
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!bindings.Variables.Any())
        {
            yield return new Set();
        }
        foreach (BindingTuple t in bindings.BindingsPattern.Tuples)
        {
            var s = new Set();
            foreach (KeyValuePair<string, PatternItem> binding in t.Values)
            {
                s.Add(binding.Key, t[binding.Key]);
            }

            yield return s;
        }
    }
}