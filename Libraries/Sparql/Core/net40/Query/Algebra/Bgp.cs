using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Graphs;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Bgp
        : IAlgebra
    {
        public Bgp()
            : this(null) {}

        public Bgp(IEnumerable<Triple> patterns)
        {
            this.TriplePatterns = patterns != null ? patterns.ToList().AsReadOnly() : new List<Triple>().AsReadOnly();
        }

        /// <summary>
        /// Gets the Triple Patterns in the BGP
        /// </summary>
        public IList<Triple> TriplePatterns { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Bgp)) return false;

            Bgp bgp = (Bgp) other;

            if (this.TriplePatterns.Count != bgp.TriplePatterns.Count) return false;

            for (int i = 0; i < this.TriplePatterns.Count; i++)
            {
                if (!this.TriplePatterns[i].Equals(bgp.TriplePatterns[i])) return false;
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

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            if (this.TriplePatterns.Count == 0) return "(bgp)";

            StringBuilder builder = new StringBuilder();
            builder.AppendLine("(bgp");
            foreach (Triple t in this.TriplePatterns)
            {
                builder.Append("  (triple ");
                builder.Append(t.Subject.ToString(formatter));
                builder.Append(' ');
                builder.Append(t.Predicate.ToString(formatter));
                builder.Append(' ');
                builder.Append(t.Object.ToString(formatter));
                builder.AppendLine(")");
            }
            builder.Append(")");
            return builder.ToString();
        }

        public IAlgebra Copy(IEnumerable<Triple> triplePatterns)
        {
            return new Bgp(triplePatterns);
        }
    }
}