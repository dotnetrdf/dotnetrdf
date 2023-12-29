using VDS.Common.Collections.Enumerations;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class LeftJoinEnumerator : IAsyncEnumerator<ISet>, IAsyncEnumerable<ISet>
{
    private readonly IAsyncEnumerator<ISet> _lhs;
    private readonly IAsyncEnumerator<ISet> _rhs;
    private bool _lhsHasMore, _rhsHasMore;
    private Task<bool> _lhsMoveNext;
    private Task<bool> _rhsMoveNext;
    private readonly LinkedList<LhsSolution> _leftSolutions;
    private readonly LinkedList<ISet> _rightSolutions;
    private IEnumerator<ISet>? _joinEnumerator;
    private readonly string[] _joinVars;
    private ISet? _current;
    
    public LeftJoinEnumerator(IAsyncEnumerator<ISet> lhs, IAsyncEnumerator<ISet> rhs, string[] joinVars)
    {
        _lhs = lhs;
        _rhs = rhs;
        _leftSolutions = new LinkedList<LhsSolution>();
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
                while (_joinEnumerator.MoveNext())
                {
                    if (_joinEnumerator.Current != null)
                    {
                        Current = _joinEnumerator.Current;
                        return true;
                    }
                }
                _joinEnumerator.Dispose();
                _joinEnumerator = null;
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
        } while (_lhsHasMore || _rhsHasMore || _joinEnumerator != null);
        return false;
    }
    private  IEnumerator<ISet> ProcessLhsSet(ISet lhSolutionSet)
    {
        var lhSolution = new LhsSolution(lhSolutionSet);
        if (_rhsHasMore)
        {
            _leftSolutions.AddLast(lhSolution);
            return _rightSolutions.Where((rhSolution) => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
                .Select(lhSolution.Join)
                .GetEnumerator();
        }

        var joinSolutions = _rightSolutions
            .Where(rhSolution => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
            .Select(lhSolution.Join).ToList();
        return joinSolutions.Count == 0 ? lhSolutionSet.AsEnumerable().GetEnumerator() : joinSolutions.GetEnumerator();
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
        _joinEnumerator = _leftSolutions.Where(s => !s.Joined).Select(s=>s.JoinIdentity()).ToList().GetEnumerator();
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

    private class LhsSolution : ISet
    {
        private readonly ISet _set;

        public LhsSolution(ISet set)
        {
            _set = set;
        }

        public bool Joined { get; private set; }

        public bool Equals(ISet other)
        {
            return _set.Equals(other);
        }

        public void Add(string variable, INode value)
        {
            _set.Add(variable, value);
        }

        public bool ContainsVariable(string variable)
        {
            return _set.ContainsVariable(variable);
        }

        public bool IsCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return _set.IsCompatibleWith(s, vars);
        }

        public bool IsMinusCompatibleWith(ISet s, IEnumerable<string> vars)
        {
            return _set.IsMinusCompatibleWith(s, vars);
        }

        public int ID
        {
            get => _set.ID;
            set => _set.ID = value;
        }

        public void Remove(string variable)
        {
            _set.Remove(variable);
        }

        public INode this[string variable] => _set[variable];

        public IEnumerable<INode> Values => _set.Values;

        public IEnumerable<string> Variables => _set.Variables;

        public ISet Join(ISet other)
        {
            Joined = true;
            return _set.Join(other);
        }

        public ISet JoinIdentity()
        {
            Joined = true;
            return _set;
        }

        public ISet Copy()
        {
            return _set.Copy();
        }

        public bool BindsAll(IEnumerable<string> vars)
        {
            return _set.BindsAll(vars);
        }
    }
}