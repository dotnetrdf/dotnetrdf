using System.Collections.Generic;
using System.Linq;
using VDS.RDF;
using VDS.RDF.Query.Algebra;

namespace dotNetRDF.Query.Behemoth
{
    public class JoinBlock : IEvaluationBlock
    {
        private readonly IEvaluationBlock _lhs;
        private readonly IEvaluationBlock _rhs;
        private readonly List<string> _joinVars;
        private readonly int _lhsWindow;
        private readonly int _rhsWindow;

        private Dictionary<int, List<Bindings>> _rhsBindingsMap = new Dictionary<int, List<Bindings>>();
        private Dictionary<int, List<Bindings>> _lhsBindingsMap = new Dictionary<int, List<Bindings>>();

        public JoinBlock(IEvaluationBlock lhs, IEvaluationBlock rhs, IEnumerable<string> joinVars, int lhsWindow = 100,
            int rhsWindow = 100)
        {
            _lhs = lhs;
            _rhs = rhs;
            _joinVars = new List<string>(joinVars);
            _lhsWindow = lhsWindow;
            _rhsWindow = rhsWindow;
        }

        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            var lhsFinished = false;
            var rhsFinished = false;
            using (IEnumerator<Bindings[]> lhsChunks = _lhs.Evaluate(input).ChunkBy(_lhsWindow).GetEnumerator())
            {
                using (IEnumerator<Bindings[]> rhsChunks = _rhs.Evaluate(input).ChunkBy(_rhsWindow).GetEnumerator())
                {
                    while (!(lhsFinished && rhsFinished))
                    {
                        if (!lhsFinished)
                        {
                            if (lhsChunks.MoveNext())
                            {
                                if (lhsChunks.Current != null)
                                {
                                    foreach (Bindings binding in lhsChunks.Current)
                                    {
                                        var joinKey = MakeJoinKey(binding);
                                        foreach (Bindings join in GetJoins(joinKey, binding, _rhsBindingsMap))
                                            yield return join;
                                        if (!rhsFinished)
                                        {
                                            InsertBinding(joinKey, binding, _lhsBindingsMap);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _rhsBindingsMap = null;
                                lhsFinished = true;
                            }
                        }

                        if (!rhsFinished)
                        {
                            if (rhsChunks.MoveNext())
                            {
                                if (rhsChunks.Current != null)
                                {
                                    foreach (Bindings binding in rhsChunks.Current)
                                    {
                                        var joinKey = MakeJoinKey(binding);
                                        foreach (Bindings join in GetJoins(joinKey, binding, _lhsBindingsMap))
                                            yield return join;
                                        if (!lhsFinished)
                                        {
                                            InsertBinding(joinKey, binding, _rhsBindingsMap);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                _lhsBindingsMap = null;
                                rhsFinished = true;
                            }
                        }
                    }
                }
            }
        }

        protected virtual IEnumerable<Bindings> GetJoins(int joinKey, Bindings thisSide,
            IReadOnlyDictionary<int, List<Bindings>> otherSideMap)
        {
            return otherSideMap.TryGetValue(joinKey, out List<Bindings> otherSideBindings)
                ? otherSideBindings.Select(thisSide.Extend)
                : Enumerable.Empty<Bindings>();
        }

        private static void InsertBinding(int joinKey, Bindings bindings, IDictionary<int, List<Bindings>> map)
        {
            if (map.TryGetValue(joinKey, out List<Bindings> list))
            {
                list.Add(bindings);
            }
            else
            {
                map[joinKey] = new List<Bindings> {bindings};
            }
        }

        private int MakeJoinKey(Bindings b)
        {
            return _joinVars.Aggregate(0, (hashCode, var) => hashCode ^ b[var].GetHashCode());
        }
    }

    public class LeftJoinBlock : IEvaluationBlock
    {
        private readonly IEvaluationBlock _lhs;
        private readonly IEvaluationBlock _rhs;
        private readonly int _lhsWindow;
        private readonly int _rhsWindow;
        private string[] _joinVars;
        private Dictionary<int, LeftJoinList> _lhsMap;
        private Dictionary<int, List<Bindings>> _rhsMap;

        public LeftJoinBlock(IEvaluationBlock lhs, IEvaluationBlock rhs, IEnumerable<string> joinVars, int lhsWindow = 100, int rhsWindow = 100)
        {
            _lhs = lhs;
            _rhs = rhs;
            _joinVars = joinVars.ToArray();
            _lhsWindow = lhsWindow;
            _rhsWindow = rhsWindow;
            _lhsMap = new Dictionary<int, LeftJoinList>();
            _rhsMap = new Dictionary<int, List<Bindings>>();
        }

        public IEnumerable<Bindings> Evaluate(Bindings input)
        {
            var lhsFinished = false;
            var rhsFinished = false;

            using (var rhsChunks = _rhs.Evaluate(input).ChunkBy(_rhsWindow).GetEnumerator())
            {
                using (var lhsChunks = _lhs.Evaluate(input).ChunkBy(_lhsWindow).GetEnumerator())
                {
                    while (!(lhsFinished && rhsFinished))
                    {
                        if (!rhsFinished)
                        {
                            if (rhsChunks.MoveNext())
                            {
                                if (rhsChunks.Current != null)
                                {
                                    foreach (var bindings in rhsChunks.Current)
                                    {
                                        var joinKey = MakeJoinKey(bindings);
                                        if (_lhsMap.TryGetValue(joinKey, out var lhsList))
                                        {
                                            foreach (Bindings lhsSolution in lhsList.Bindings.Where(lhsSolution => IsJoin(lhsSolution, bindings)))
                                            {
                                                yield return lhsSolution.Extend(bindings);
                                                lhsList.Joined = true;
                                            }
                                        }

                                        if (!lhsFinished)
                                        {
                                            if (_rhsMap.TryGetValue(joinKey, out var bindingsList))
                                            {
                                                bindingsList.Add(bindings);
                                            }
                                            else
                                            {
                                                _rhsMap[joinKey] = new List<Bindings> {bindings};
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                rhsFinished = true;
                                // Flush any unjoined values from lhsMap
                                foreach (var unjoined in _lhsMap.Values.Where(x => !x.Joined))
                                {
                                    foreach (var solution in unjoined.Bindings)
                                    {
                                        yield return solution;
                                    }
                                }

                                _lhsMap = null;
                            }
                        }

                        if (!lhsFinished)
                        {
                            if (lhsChunks.MoveNext())
                            {
                                if (lhsChunks.Current != null)
                                {
                                    foreach (Bindings lhsSolution in lhsChunks.Current)
                                    {
                                        var joinKey = MakeJoinKey(lhsSolution);
                                        if (_rhsMap.TryGetValue(joinKey, out List<Bindings> rhsSolutions))
                                        {
                                            var joined = false;
                                            foreach (Bindings rhsSolution in rhsSolutions.Where(x => IsJoin(lhsSolution, x)))
                                            {
                                                yield return lhsSolution.Extend(rhsSolution);
                                                joined = true;
                                            }

                                            if (_lhsMap.TryGetValue(joinKey, out LeftJoinList leftJoinList))
                                            {
                                                leftJoinList.Add(lhsSolution, joined);
                                            }
                                            else
                                            {
                                                _lhsMap[joinKey] = new LeftJoinList(lhsSolution, joined);
                                            }

                                        }
                                    }
                                }
                            }
                            else
                            {
                                lhsFinished = true;
                                _rhsMap = null;
                            }
                        }
                    }
                }
            }
        }

        private int MakeJoinKey(Bindings b)
        {
            return _joinVars.Aggregate(0, (hashCode, var) => hashCode ^ b[var].GetHashCode());
        }

        private bool IsJoin(Bindings x, Bindings y)
        {
            return _joinVars.All(v => x.ContainsVariable(v) && y.ContainsVariable(v) && x[v].Equals(y[v]));
        }

        private class LeftJoinList
        {
            public List<Bindings> Bindings { get; }
            public bool Joined { get; set; }

            public LeftJoinList(Bindings bindings, bool joined)
            {
                Bindings = new List<Bindings> {bindings};
                Joined = joined;
            }

            public void Add(Bindings bindings, bool joined)
            {
                Bindings.Add(bindings);
                Joined |= joined;
            }

            
        }
    }
}
