using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VDS.RDF.Query.Engine.Joins.Workers
{
    public class FixedHashJoinWorker
        : ReusableJoinWorker
    {
        public FixedHashJoinWorker(IList<String> joinVars, IEnumerable<ISolution> rhs)
        {
            if (joinVars == null) throw new ArgumentNullException("joinVars");
            this.JoinVariables = joinVars is ReadOnlyCollection<String> ? joinVars : new List<string>(joinVars).AsReadOnly();
            if (this.JoinVariables.Count == 0) throw new ArgumentException("Number of join variables must be >= 1", "joinVars");
            
            // Build the hash
            this.Hash = new Dictionary<ISolution, List<ISolution>>(new SetDistinctnessComparer(this.JoinVariables));
            foreach (ISolution s in rhs)
            {
                List<ISolution> sets;
                if (!this.Hash.TryGetValue(s, out sets))
                {
                    sets = new List<ISolution>();
                    this.Hash.Add(s, sets);
                }
                sets.Add(s);
            }
        }

        private IDictionary<ISolution, List<ISolution>> Hash { get; set; }

        public IList<String> JoinVariables { get; private set; } 

        public override IEnumerable<ISolution> Find(ISolution lhs, IExecutionContext context)
        {
            List<ISolution> sets;
            return this.Hash.TryGetValue(lhs, out sets) ? sets.Where(s => lhs.IsCompatibleWith(s, this.JoinVariables)) : Enumerable.Empty<ISolution>();
        }
    }
}
