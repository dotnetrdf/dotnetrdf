using System.Runtime.CompilerServices;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncMinusEvaluation : IAsyncEvaluation
{
    private readonly IAsyncEvaluation _lhs;
    private readonly IAsyncEvaluation _rhs;
    private readonly List<string> _minusVars;
    
    public AsyncMinusEvaluation(Minus minus, IAsyncEvaluation lhs, IAsyncEvaluation rhs)
    {
        Minus minus1 = minus;
        _minusVars = minus1.Lhs.Variables.Intersect(minus1.Rhs.Variables).ToList();
        _lhs = lhs;
        _rhs = rhs;
    }
    public async IAsyncEnumerable<ISet> Evaluate(PullEvaluationContext context, ISet? input, IRefNode? activeGraph,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        // TODO: Is there a way to optimise this to either avoid computing a full list of rhs bindings or to store them in an indexed structure to make IsCompatible more efficient?
        List<ISet> rhsSolutions = await _rhs.Evaluate(context, input, activeGraph, cancellationToken).ToListAsync(cancellationToken);
        await foreach (ISet? lhsSolution in _lhs.Evaluate(context, input, activeGraph, cancellationToken))
        {
            if (rhsSolutions.Any(r => IsCompatible(lhsSolution, r, _minusVars)))
            {
                continue;
            }
            yield return lhsSolution;
        }
    }

    private bool IsCompatible(ISet l, ISet r, List<string> vars)
    {
        var testVars = vars.Where(v => l[v] != null && r[v] != null).ToList();
        if (!testVars.Any()) return false;
        return testVars.All(v => l[v].Equals(r[v]));
    }
}