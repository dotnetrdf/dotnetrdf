using VDS.Common.Collections;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class JoinIndex
{
    private readonly string[] _joinVars;
    private readonly List<ISet> _indexedSets = new();
    private readonly List<MultiDictionary<INode, HashSet<int>>> _index;
    private readonly List<HashSet<int>> _nulls;

    public JoinIndex(string[] joinVars)
    {
        _joinVars = joinVars;
        _index = Enumerable.Range(0, joinVars.Length).Select(_ => new MultiDictionary<INode, HashSet<int>>()).ToList();
        _nulls = Enumerable.Range(0, joinVars.Length).Select(_ => new HashSet<int>()).ToList();
    }

    public void Add(ISet s)
    {
        _indexedSets.Add(s);
        var setIndex = _indexedSets.Count - 1;
        for (var i = 0; i < _joinVars.Length; i++)
        {
            var var = _joinVars[i];
            MultiDictionary<INode, HashSet<int>> varIndex = _index[i];
            if (s.ContainsVariable(var))
            {
                INode value = s[var];
                if (!varIndex.TryGetValue(value, out HashSet<int>? sets))
                {
                    sets = new HashSet<int>();
                    varIndex.Add(value, sets);
                }
                sets.Add(setIndex);
            }
            else
            {
                _nulls[i].Add(setIndex);
            }
        }
    }

    private HashSet<int> GetMatchesForVar(int varIx, ISet s, bool matchNulls = false)
    {
        var v = _joinVars[varIx];
        if (!s.ContainsVariable(v))
        {
            return [];
        }
        INode value = s[v];
        if (value == null)
        {
            return _nulls[varIx]; 
        }
        HashSet<int> results = matchNulls ? [.._nulls[varIx]] : [];
        if (_index[varIx].TryGetValue(value, out HashSet<int>? sets))
        {
            results.UnionWith(sets);
        }

        return results;
    }

    public IEnumerable<ISet> GetMatches(ISet s)
    {
        HashSet<int>? results = null;
        for (var i = 0; i < _joinVars.Length; i++)
        {
            if (s.ContainsVariable(_joinVars[i]))
            {
                if (results == null)
                {
                    results = GetMatchesForVar(i, s);
                }
                else
                {
                    results.IntersectWith(GetMatchesForVar(i, s));
                }
            }
        }

        return results?.Select(ix => _indexedSets[ix]) ?? [];
    }
}