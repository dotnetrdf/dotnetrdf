using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class JoinEnumerator : IAsyncEnumerator<ISet>, IAsyncEnumerable<ISet>
{
    private readonly IAsyncEnumerator<ISet> _lhs;
    private readonly IAsyncEnumerator<ISet> _rhs;
    private bool _lhsHasMore, _rhsHasMore;
    private Task<bool> _lhsMoveNext;
    private Task<bool> _rhsMoveNext;
    private readonly LinkedList<ISet> _leftSolutions;
    private readonly LinkedList<ISet> _rightSolutions;
    private IEnumerator<ISet>? _joinEnumerator;
    private readonly string[] _joinVars;
    private ISet? _current;

    public JoinEnumerator(IAsyncEnumerator<ISet> lhs, IAsyncEnumerator<ISet> rhs, string[] joinVars)
    {
        _lhs = lhs;
        _rhs = rhs;
        _leftSolutions = new LinkedList<ISet>();
        _rightSolutions = new LinkedList<ISet>();
        _lhsMoveNext = lhs.MoveNextAsync().AsTask();
        _rhsMoveNext = rhs.MoveNextAsync().AsTask();
        _lhsHasMore = true;
        _rhsHasMore = true;
        _joinEnumerator = null;
        _joinVars = joinVars;
        _current = null;
    }

    /// <inheritdoc />
    public IAsyncEnumerator<ISet> GetAsyncEnumerator(CancellationToken cancellationToken = new CancellationToken())
    {
        return this;
    }

    /// <inheritdoc />
    public async ValueTask<bool> MoveNextAsync()
    {
        do
        {
            if (_joinEnumerator != null)
            {
                if (_joinEnumerator.MoveNext())
                {
                    Current = _joinEnumerator.Current;
                    return true;
                }
                else
                {
                    _joinEnumerator.Dispose();
                    _joinEnumerator = null;
                }
            }

            if (_lhsHasMore)
            {
                if (_rhsHasMore)
                {
                    var completed = Task.WaitAny(_lhsMoveNext, _rhsMoveNext);
                    if (completed == 0)
                    {
                        _lhsHasMore = _lhsMoveNext.Result;
                        if (_lhsHasMore)
                        {
                            _joinEnumerator = ProcessLhsSet(_lhs.Current);
                            _lhsMoveNext = _lhs.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            OnLhsDone();
                        }
                    }
                    else
                    {
                        _rhsHasMore = _rhsMoveNext.Result;
                        if (_rhsHasMore)
                        {
                            _joinEnumerator = ProcessRhsSet(_rhs.Current);
                            _rhsMoveNext = _rhs.MoveNextAsync().AsTask();
                        }
                        else
                        {
                            OnRhsDone();
                        }
                    }
                }
                else
                {
                    _lhsHasMore = await _lhsMoveNext;
                    if (_lhsHasMore)
                    {
                        _joinEnumerator = ProcessLhsSet(_lhs.Current);
                        _lhsMoveNext = _lhs.MoveNextAsync().AsTask();
                    }
                    else
                    {
                        OnLhsDone();
                    }
                }
            }
            else if (_rhsHasMore)
            {
                _rhsHasMore = await _rhsMoveNext;
                if (_rhsHasMore)
                {
                    _joinEnumerator = ProcessRhsSet(_rhs.Current);
                    _rhsMoveNext = _rhs.MoveNextAsync().AsTask();
                }
                else
                {
                    OnRhsDone();
                }
            }
        } while (_lhsHasMore || _rhsHasMore);

        return false;
    }
    
    private  IEnumerator<ISet> ProcessLhsSet(ISet lhSolution)
    {
        if (_rhsHasMore)
        {
            _leftSolutions.AddLast(lhSolution);
        }
        return _rightSolutions.Where((rhSolution) => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
            .Select(lhSolution.Join)
            .GetEnumerator();
    }

    private IEnumerator<ISet> ProcessRhsSet(ISet rhSolution)
    {
        if (_lhsHasMore)
        {
            _rightSolutions.AddLast(rhSolution);
        }

        return _leftSolutions.Where(lhSolution => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
            .Select(lhSolution => lhSolution.Join(rhSolution))
            .GetEnumerator();
    }

    private void OnLhsDone()
    {
        _rightSolutions.Clear();
    }

    private void OnRhsDone()
    {
        _leftSolutions.Clear();
    }

    /// <inheritdoc />
    public ISet Current
    {
        get
        {
            if (_current == null) throw new InvalidOperationException();
            return _current;
        }
        private set { _current = value; }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _lhs.DisposeAsync();
        await _rhs.DisposeAsync();
        _joinEnumerator?.Dispose();
    }
}


