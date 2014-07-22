using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Graphs;
using VDS.RDF.Query.Algebra;
using VDS.RDF.Query.Engine.Bgps;
using VDS.RDF.Query.Engine.Joins.Strategies;

namespace VDS.RDF.Query.Engine.Algebra
{
    public class MedusaAlgebraExecutor
        : BaseAlgebraExecutor
    {
        public MedusaAlgebraExecutor(IBgpExecutor executor)
            : this(executor, new DefaultJoinStrategySelector()) { }

        public MedusaAlgebraExecutor(IBgpExecutor executor, IJoinStrategySelector joinStrategySelector)
            : base(joinStrategySelector)
        {
            if (executor == null) throw new ArgumentNullException("executor");
            this.BgpExecutor = executor;
        }

        public IBgpExecutor BgpExecutor { get; private set; }

        public override IEnumerable<ISolution> Execute(Bgp bgp, IExecutionContext context)
        {
            context = EnsureContext(context);

            // An empty BGP acts as an identity and always matches producing a single empty set
            List<Triple> patterns = bgp.TriplePatterns.ToList();
            if (patterns.Count == 0) return new Solution().AsEnumerable();

            // Otherwise build up the enumerable by sequencing the triple pattern matches together
            // TODO Should the query engine schedule the order of patterns or are we expecting the optimizer to do that for us?
            IEnumerable<ISolution> results = Enumerable.Empty<ISolution>();
            for (int i = 0; i < patterns.Count; i++)
            {
                if (i == 0)
                {
                    // Initial match is unbounded
                    results = this.BgpExecutor.Match(patterns[i], context);
                }
                else
                {
                    // Subsequent matches are bounded by the data returned from previous matches
                    // Essentially we are performing an index join
                    int i1 = i;
                    results = results.SelectMany(s => this.BgpExecutor.Match(patterns[i1], s, context));
                }
            }
            return results;
        }
    }
}