/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

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