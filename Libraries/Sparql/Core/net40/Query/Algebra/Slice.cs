using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Algebra
{
    public class Slice
        : IUnaryAlgebra
    {
        public Slice(IAlgebra innerAlgebra, long limit, long offset)
        {
            if (innerAlgebra == null) throw new ArgumentNullException("innerAlgebra", "Inner Algebra cannot be null");
            this.InnerAlgebra = innerAlgebra;
            this.Limit = limit >= 0 ? limit : -1;
            this.Offset = offset > 0 ? offset : 0;
        }

        public IAlgebra InnerAlgebra { get; private set; }

        public long Limit { get; private set; }

        public long Offset { get; private set; }

        public bool Equals(IAlgebra other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is Slice)) return false;

            Slice s = (Slice) other;
            return this.Limit == s.Limit && this.Offset == s.Offset;
        }

        public void Accept(IAlgebraVisitor visitor)
        {
            visitor.Visit(this);
        }
    }
}