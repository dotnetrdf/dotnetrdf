using System;
using System.Collections.Generic;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Algebra
{
    public class Slice
        : BaseUnaryAlgebra
    {
        public Slice(IAlgebra innerAlgebra, long limit, long offset)
            : base(innerAlgebra)
        {
            this.Limit = limit >= 0 ? limit : -1;
            this.Offset = offset > 0 ? offset : 0;
        }

        public long Limit { get; private set; }

        public long Offset { get; private set; }

        public override bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Slice)) return false;

            Slice s = (Slice) other;
            return this.Limit == s.Limit && this.Offset == s.Offset;
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
            builder.Append("(slice ");
            if (this.Offset > 0)
            {
                builder.Append(this.Offset);
            }
            else
            {
                builder.Append('_');
            }
            builder.Append(' ');
            if (this.Limit > 0)
            {
                builder.Append(this.Limit);
            }
            else
            {
                builder.Append('_');
            }
            builder.AppendLine();
            builder.AppendLineIndented(this.InnerAlgebra.ToString(formatter), 2);
            builder.AppendLine(")");
            return builder.ToString();
        }

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new Slice(innerAlgebra, this.Limit, this.Offset);
        }
    }
}