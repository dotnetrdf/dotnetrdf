using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Paths;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class PropertyPath
        : BaseUnaryAlgebra
    {
        public PropertyPath(IAlgebra innerAlgebra, TriplePath triplePath)
            : base(innerAlgebra)
        {
            if (triplePath == null) throw new ArgumentNullException("triplePath");
            this.TriplePath = triplePath;
        }

        public TriplePath TriplePath { get; private set; }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new PropertyPath(innerAlgebra, this.TriplePath);
        }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.InnerAlgebra.ProjectedVariables.Concat(this.TriplePath.Variables).Distinct(); }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.InnerAlgebra.FixedVariables.Concat(this.TriplePath.Variables).Distinct(); }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.InnerAlgebra.FloatingVariables.Except(this.FixedVariables); }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.Append("(path ");
            builder.Append(formatter.Format(this.TriplePath.Subject));
            builder.Append(" (");
            builder.Append(this.TriplePath.Path.ToString(formatter));
            builder.Append(") ");
            builder.AppendLine(formatter.Format(this.TriplePath.Object));
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is PropertyPath)) return false;

            PropertyPath pp = (PropertyPath) other;

            return this.TriplePath.Equals(pp.TriplePath);
        }
    }
}
