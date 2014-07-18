using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    public class FixedHashJoinWorker
        : ReusableJoinWorker
    {
        public FixedHashJoinWorker(IList<String> joinVars, IEnumerable<ISet> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1", "joinVars");
            
            // Build the hash
            this.Hash = new Dictionary<ISet, List<ISet>>(new SetDistinctnessComparer(this.JoinVariables));
            foreach (ISet s in rhs)
            {
                List<ISet> sets;
                if (!this.Hash.TryGetValue(s, out sets))
                {
                    sets = new List<ISet>();
                    this.Hash.Add(s, sets);
                }
                sets.Add(s);
            }
        }

        private IDictionary<ISet, List<ISet>> Hash { get; set; }

        public IList<String> JoinVariables { get; private set; } 

        public override IEnumerable<ISet> Find(ISet lhs, IExecutionContext context)
        {
            List<ISet> sets;
            return this.Hash.TryGetValue(lhs, out sets) ? sets.Where(s => lhs.IsCompatibleWith(s, this.JoinVariables)) : Enumerable.Empty<ISet>();
        }
    }
}
