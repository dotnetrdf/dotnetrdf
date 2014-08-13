using System;
using System.Collections;
using System.Collections.Generic;
using VDS.RDF.Query.Grouping;

namespace VDS.RDF.Query.Engine.Grouping
{
    public class SolutionGroup
        : Solution, ISolutionGroup
    {
        public SolutionGroup(ISolution solution, IEnumerable<KeyValuePair<String, IAccumulator>> aggregates)
            : base(solution)
        {
            if (aggregates == null) throw new ArgumentNullException("aggregates");
            this.Accumulators = new Dictionary<string, IAccumulator>();
            foreach (KeyValuePair<String, IAccumulator> aggregate in aggregates)
            {
                this.Accumulators.Add(aggregate);
            }
        }

        public IDictionary<string, IAccumulator> Accumulators { get; set; }

        public void FinalizeGroup()
        {
            foreach (KeyValuePair<String, IAccumulator> aggregate in this.Accumulators)
            {
                this.Add(aggregate.Key, aggregate.Value.AccumulatedResult);
            }
        }
    }
}
