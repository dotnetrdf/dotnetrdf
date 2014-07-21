using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Sorting;

namespace VDS.RDF.Query.Algebra
{
    public class OrderBy
        : BaseUnaryAlgebra
    {
        public OrderBy(IAlgebra innerAlgebra, IEnumerable<ISortCondition> sortConditions)
            : base(innerAlgebra)
        {
            this.SortConditions = sortConditions.ToList().AsReadOnly();
            if (this.SortConditions.Count == 0) throw new ArgumentException("Number of sort conditions must be >= 1", "sortConditions");
        }

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
            if (!(other is OrderBy)) return false;

            OrderBy order = (OrderBy) other;
            if (this.SortConditions.Count != order.SortConditions.Count) return false;
            for (int i = 0; i < this.SortConditions.Count; i++)
            {
                if (!this.SortConditions[i].Equals(order.SortConditions[i])) return false;
            }
            return this.InnerAlgebra.Equals(order.InnerAlgebra);
        }
    }
}
