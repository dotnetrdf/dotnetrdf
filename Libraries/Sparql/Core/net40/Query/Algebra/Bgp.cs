using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Algebra
{
    public class Bgp 
        : IAlgebra
    {

        public Bgp()
        {
            this.TriplePatterns = new ImmutableView<Triple>();
        }

        public Bgp(IEnumerable<Triple> patterns)
        {
            this.TriplePatterns = patterns != null ? new MaterializedImmutableView<Triple>(patterns) : new ImmutableView<Triple>();
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        public IEnumerable<Triple> TriplePatterns { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Bgp)) return false;

            List<Triple> ts = this.TriplePatterns.ToList();
            List<Triple> otherTriples = ((Bgp) other).TriplePatterns.ToList();
            if (ts.Count != otherTriples.Count) return false;

            for (int i = 0; i < ts.Count; i++)
            {
                if (!ts[i].Equals(otherTriples[i])) return false;
            }
            return true;
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISet> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }
    }
}
