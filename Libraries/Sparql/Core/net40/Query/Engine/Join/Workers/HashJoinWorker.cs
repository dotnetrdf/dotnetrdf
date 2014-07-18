using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VDS.RDF.Query.Engine.Join.Workers
{
    public class HashJoinWorker
        : ReusableJoinWorker
    {
        public HashJoinWorker(IList<String> joinVars, IEnumerable<ISet> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            
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

        public override IEnumerable<ISet> Find(ISet lhs)
        {
            List<ISet> sets;
            return this.Hash.TryGetValue(lhs, out sets) ? sets : Enumerable.Empty<ISet>();
        }
    }
}
