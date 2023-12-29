using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Patterns;

namespace dotNetRdf.Query.PullEvaluation;

public class TriplePatternEnumerator : IAsyncEnumerator<ISet>
{
    private readonly IMatchTriplePattern _matchTriplePattern;
    private readonly PullEvaluationContext _context;
    private IEnumerator<ISet>? _results;

    public TriplePatternEnumerator(IMatchTriplePattern t, PullEvaluationContext context)
    {
        _matchTriplePattern = t;
        _context = context;
    }

    public async ValueTask<bool> MoveNextAsync()
    {
        _results ??= await Task.Run(() => _context.GetTriples(_matchTriplePattern, null).Select(t => _matchTriplePattern.Evaluate(_context, t)).Where(s => s != null).GetEnumerator());
        return _results.MoveNext();
    }

    public ISet Current
    {
        get
        {
            if (_results == null) throw new InvalidOperationException();
            return _results.Current;
        }
    }

    public async ValueTask DisposeAsync()
    {
        _results?.Dispose();
    }
}