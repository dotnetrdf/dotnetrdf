using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Engine.Algebra;
using VDS.RDF.Query.Sorting;
using VDS.RDF.Writing.Formatting;

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

        public override IAlgebra Copy(IAlgebra innerAlgebra)
        {
            return new OrderBy(innerAlgebra, this.SortConditions);
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
            builder.Append("(order (");
            foreach (ISortCondition condition in this.SortConditions)
            {
                builder.Append(' ');
                builder.Append(condition.ToString(formatter));
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
