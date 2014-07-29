using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Minus
        : BaseBinaryAlgebra
    {
        public Minus(IAlgebra lhs, IAlgebra rhs) 
            : base(lhs, rhs) { }

        public override IEnumerable<string> ProjectedVariables
        {
            get { return this.Lhs.ProjectedVariables; }
        }

        public override IEnumerable<string> FixedVariables
        {
            get { return this.Lhs.FixedVariables; }
        }

        public override IEnumerable<string> FloatingVariables
        {
            get { return this.Lhs.FloatingVariables; }
        }

        public override void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override IEnumerable<ISolution> Execute(IAlgebraExecutor executor, IExecutionContext context)
        {
            return executor.Execute(this, context);
        }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Minus)) return false;

            Minus m = (Minus) other;
            return this.Lhs.Equals(m.Lhs) && this.Rhs.Equals(m.Rhs);
        }

        public override string ToString(IAlgebraFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("(minus");
            builder.AppendLineIndented(this.Lhs.ToString(formatter), 2);
            builder.AppendLineIndented(this.Rhs.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }
    }
}
