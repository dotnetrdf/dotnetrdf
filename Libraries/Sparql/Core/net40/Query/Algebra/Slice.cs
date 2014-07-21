using System.Collections.Generic;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;

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
    }
}