using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRdf.Query.PullEvaluation;

internal class AsyncLeftJoinEvaluation : AbstractAsyncJoinEvaluation
{
    private readonly string[] _joinVars;
    private readonly LinkedList<LhsSolution> _leftSolutions;
    private readonly LinkedList<ISet> _rightSolutions;

    
    public AsyncLeftJoinEvaluation(IAsyncEvaluation lhs, IAsyncEvaluation rhs, string[] joinVars) :
        base(lhs, rhs)
    {
        _joinVars = joinVars;
        _leftSolutions = new LinkedList<LhsSolution>();
        _rightSolutions = new LinkedList<ISet>();
    }
    
    protected override  IEnumerable<ISet> ProcessLhs(ISet lhSolutionSet)
    {
        var lhSolution = new LhsSolution(lhSolutionSet);
        if (_rhsHasMore)
        {
            _leftSolutions.AddLast(lhSolution);
            return _rightSolutions.Where((rhSolution) => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
                .Select(lhSolution.Join);
        }

        var joinSolutions = _rightSolutions
            .Where(rhSolution => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
            .Select(lhSolution.Join).ToList();
        return joinSolutions.Count == 0 ? lhSolutionSet.AsEnumerable() : joinSolutions;
    }

    protected override IEnumerable<ISet> ProcessRhs(ISet rhSolution)
    {
        if (_lhsHasMore)
        {
            _rightSolutions.AddLast(rhSolution);
        }

        return _leftSolutions.Where(lhSolution => lhSolution.IsCompatibleWith(rhSolution, _joinVars))
            .Select(lhSolution => lhSolution.Join(rhSolution));
    }

    protected override IEnumerable<ISet>? OnLhsDone()
    {
        _rightSolutions.Clear();
        return null;
    }

    protected override IEnumerable<ISet> OnRhsDone()
    {
        IList<ISet> addResults = _leftSolutions.Where(s => !s.Joined).Select(s=>s.JoinIdentity()).ToList();
        _leftSolutions.Clear();
        return addResults;
    }

    private class LhsSolution : ISet
    {
        private readonly ISet _set;

        public LhsSolution(ISet set)
        {
            _set = set;
        }

        public bool Joined { get; private set; }

        public bool Equals(ISet? other)
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