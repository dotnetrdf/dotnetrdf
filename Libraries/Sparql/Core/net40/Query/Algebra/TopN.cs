using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Algebra
{
    public class TopN
        : BaseUnaryAlgebra
    {
        public TopN(IAlgebra innerAlgebra, IEnumerable<ISortCondition> conditions, long n)
            : base(innerAlgebra)
        {
            this.SortConditions = conditions.ToList().AsReadOnly();
            if (this.SortConditions.Count == 0) throw new ArgumentException("Number of sort conditions must be >= 1", "conditions");
            if (n < 1) throw new ArgumentException("N must be >= 1", "n");
            this.N = n;
        }

        public long N { get; set; }

        public IList<ISortCondition> SortConditions { get; private set; }

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
            if (!(other is TopN)) return false;

            TopN top = (TopN) other;
            if (this.N != top.N) return false;

            if (this.SortConditions.Count != top.SortConditions.Count) return false;
            for (int i = 0; i < this.SortConditions.Count; i++)
            {
                if (!this.SortConditions[i].Equals(top.SortConditions[i])) return false;
            }
            return this.InnerAlgebra.Equals(top.InnerAlgebra);
        }
    }
}