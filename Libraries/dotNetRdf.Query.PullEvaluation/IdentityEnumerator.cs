using System.Transactions;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

/// <summary>
/// Represents the join identity of a single empty solution mapping.
/// </summary>
public class IdentityEnumerator : IAsyncEnumerator<ISet>
{
    private bool _started = false;
    private readonly ISet _empty = new Set();

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return new ValueTask();
    }


    /// <inheritdoc />
    public ValueTask<bool> MoveNextAsync()
    {
        if (_started)
        {
            return new ValueTask<bool>(false);
        }

        _started = true;
        return new ValueTask<bool>(true);
    }

    /// <inheritdoc />
    public ISet Current
    {
        get
        {
            if (!_started) { throw new InvalidOperationException(); }
            return _empty;
        }
    }
}