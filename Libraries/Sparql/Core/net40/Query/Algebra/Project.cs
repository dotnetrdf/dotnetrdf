using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Project
        : BaseUnaryAlgebra
    {

        public Project(IAlgebra innerAlgebra, IEnumerable<String> vars)
            : base(innerAlgebra)
        {
            this.Projections = vars.ToList().AsReadOnly();
            if (this.Projections.Count == 0) throw new ArgumentException("Number of variables to project must be >= 1", "vars");
        }

        public IList<String> Projections { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new Project(innerAlgebra, this.Projections);
        }

        public override IEnumerable<string> ProjectedVariables { get { return this.Projections; } }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.InnerAlgebra.FixedVariables.Where(v => this.Projections.Contains(v)); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.Projections.Except(this.FixedVariables); }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append("(project (");
            foreach (String var in this.Projections)
            {
                builder.Append(' ');
                builder.Append(formatter.Format(new VariableNode(var)));
            }
            builder.AppendLine(")");
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Project)) return false;

            Project p = (Project) other;
            if (this.Projections.Count != p.Projections.Count) return false;
            for (int i = 0; i < this.Projections.Count; i++)
            {
                if (!this.Projections[i].Equals(p.Projections[i], StringComparison.Ordinal)) return false;
            }
            return this.InnerAlgebra.Equals(p.InnerAlgebra);
        }
    }
}
