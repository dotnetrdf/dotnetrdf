using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class AsyncBindingsEvaluation : IAsyncEvaluation
{
    private readonly Bindings _bindings;
    public AsyncBindingsEvaluation(Bindings bindings)
    {
        _bindings = bindings;
    }
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (!_bindings.Variables.Any())
        {
            yield return new Set();
        }
        foreach (BindingTuple t in _bindings.BindingsPattern.Tuples)
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