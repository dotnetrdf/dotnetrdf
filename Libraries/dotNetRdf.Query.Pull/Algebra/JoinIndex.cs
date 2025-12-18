using VDS.Common.Collections;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace VDS.RDF.Query.Pull.Algebra;

internal class JoinIndex
{
    private readonly string[] _joinVars;
    private readonly List<ISet> _indexedSets = [];
    private readonly List<MultiDictionary<INode, HashSet<int>>> _index;
    private readonly List<HashSet<int>> _nulls;
    private readonly bool _trackJoins;
    private HashSet<int> unjoinedSets = [];

    public JoinIndex(string[] joinVars, bool trackJoins = false)
    {
        _joinVars = joinVars;
        _index = Enumerable.Range(0, joinVars.Length).Select(_ => new MultiDictionary<INode, HashSet<int>>()).ToList();
        _nulls = Enumerable.Range(0, joinVars.Length).Select(_ => new HashSet<int>()).ToList();
        _trackJoins = trackJoins;
    }

    public int Add(ISet s)
    {
        _indexedSets.Add(s);
        var setIndex = _indexedSets.Count - 1;
        if (_trackJoins)
        {
            unjoinedSets.Add(setIndex);
        }
        for (var i = 0; i < _joinVars.Length; i++)
        {
            var var = _joinVars[i];
            MultiDictionary<INode, HashSet<int>> varIndex = _index[i];
            if (s.ContainsVariable(var))
            {
                INode value = s[var];
                if (value == null)
                {
                    _nulls[i].Add(setIndex);
                }
                else
                {
                    if (!varIndex.TryGetValue(value, out HashSet<int>? sets))
                    {
                        sets = [];
                        varIndex.Add(value, sets);
                    }

                    sets.Add(setIndex);
                }
            }
            else
            {
                _nulls[i].Add(setIndex);
            }
        }
        return setIndex;
    }

    private HashSet<int> GetMatchesForVar(int varIx, ISet s, bool matchNulls = true)
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
        HashSet<int> results = matchNulls ? [.. _nulls[varIx]] : [];
        if (_index[varIx].TryGetValue(value, out HashSet<int>? sets))
        {
            results.UnionWith(sets);
        }

        return results;
    }

    public IEnumerable<int> GetMatchIndexes(ISet s)
    {
        HashSet<int>? results = null;
        for (var i = 0; i < _joinVars.Length; i++)
        {
            if (s.ContainsVariable(_joinVars[i]))
            {
                if (s[_joinVars[i]] == null)
                {
                    continue;
                }
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
        return results ?? [];
    }

    public ISet this[int index] => _indexedSets[index];

    public void MarkJoined(int index)
    {
        if (_trackJoins)
        {
            unjoinedSets.Remove(index);
        }
    }

    public IEnumerable<ISet> GetMatches(ISet s)
    {
        HashSet<int>? results = null;
        for (var i = 0; i < _joinVars.Length; i++)
        {
            if (s.ContainsVariable(_joinVars[i]))
            {
                if (s[_joinVars[i]] == null)
                {
                    continue;
                }
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
        if (results == null) return [];
        if (_trackJoins)
        {
            unjoinedSets.ExceptWith(results);
        }
        return results.Select(ix => _indexedSets[ix]);
    }
    
    public IEnumerable<ISet> GetUnjoinedSets()
    {
        if (!_trackJoins)
        {
            throw new InvalidOperationException("Cannot get unjoined sets from a JoinIndex that is not tracking joins");
        }
        return unjoinedSets.Select(ix => _indexedSets[ix]);
    }
}