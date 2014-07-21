using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class FloatingHashJoinWorker
        : ReusableJoinWorker
    {
        public FloatingHashJoinWorker(IList<String> joinVars, IEnumerable<ISolution> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1", "joinVars");
            
            // Build the hash
            this.Hash = new List<IDictionary<INode, IList<int>>>();
            this.Nulls = new List<IList<int>>();
            foreach (String var in this.JoinVariables)
            {
                this.Hash.Add(new Dictionary<INode, IList<int>>());
                this.Nulls.Add(new List<int>());
            }
            this.Sets = new List<ISolution>();
            foreach (ISolution s in rhs)
            {
                int id = this.Sets.Count;
                this.Sets.Add(s);

                for (int i = 0; i < this.JoinVariables.Count; i++)
                {
                    INode value = s[this.JoinVariables[i]];
                    if (value != null)
                    {
                        IList<int> ids;
                        if (!this.Hash[i].TryGetValue(value, out ids))
                        {
                            ids = new List<int>();
                            this.Hash[i].Add(value, ids);
                        }
                        ids.Add(id);
                    }
                    else
                    {
                        this.Nulls[i].Add(id);
                    }
                }
            }
        }

        private IList<ISolution> Sets { get; set; } 

        private IList<IDictionary<INode, IList<int>>> Hash { get; set; }

        private IList<IList<int>> Nulls { get; set; }

        public IList<String> JoinVariables { get; private set; } 

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            // If no RHS sets then there can't be any matches
            if (this.Sets.Count == 0) return Enumerable.Empty<ISolution>();

            // Otherwise use the hashes to find the possible matches
            IEnumerable<int> possMatches = null;
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1");
            for (int i = 0; i < this.JoinVariables.Count; i++)
            {
                INode value = lhs[this.JoinVariables[i]];
                if (value != null)
                {
                    if (this.Hash[i].ContainsKey(value))
                    {
                        // Some RHS matches for the current variable, intersect these with the current set of possible matches
                        // Remember that RHS nulls also match this value
                        possMatches = (possMatches == null ? this.Hash[i][value].Concat(this.Nulls[i]) : possMatches.Intersect(this.Hash[i][value].Concat(this.Nulls[i])));
                    }
                    else
                    {
                        // No RHS matches for the value of the current variable so there are no compatible solutions
                        possMatches = Enumerable.Empty<int>();
                        break;
                    }
                }
                else
                {
                    // Don't forget that a null will be potentially compatible with everything
                    possMatches = (possMatches == null ? Enumerable.Range(0, this.Sets.Count) : possMatches.Intersect(Enumerable.Range(0, this.Sets.Count)));
                }
            }

            // Make sure to filter for compatibility
            return possMatches.Select(id => this.Sets[id]).Where(s => lhs.IsCompatibleWith(s, this.JoinVariables));
        }
    }
}
