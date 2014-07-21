using System;
using System.Collections.Generic;
using System.Linq;
using VDS.Common.Collections;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

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

        public IEnumerable<string> ProjectedVariables
        {
            get { return this.TriplePatterns.SelectMany(t => t.Nodes).Where(n => n.NodeType == NodeType.Variable).Select(n => n.VariableName).Distinct(); }
        }

        public IEnumerable<string> FixedVariables
        {
            get { return this.ProjectedVariables; }
        }

        public IEnumerable<string> FloatingVariables
        {
            get { return Enumerable.Empty<String>(); }
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }
    }
}