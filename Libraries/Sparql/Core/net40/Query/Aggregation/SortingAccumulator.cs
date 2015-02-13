using System;
using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    /// <summary>
    /// An accumulator that accumulates a single value decided which value to keep based on a given comparer
    /// </summary>
    public class SortingAccumulator
        : BaseExpressionAccumulator
    {
        public SortingAccumulator(IExpression expr, IComparer<IValuedNode> comparer)
            : this(expr, comparer, null) { }

        public SortingAccumulator(IExpression expr, IComparer<IValuedNode> comparer, IValuedNode initialValue)
            : base(expr, initialValue)
        {
            if (comparer == null) throw new ArgumentNullException("comparer");
            this.Comparer = comparer;
        }

        public IComparer<IValuedNode> Comparer { get; private set; }

        public override bool Equals(IAccumulator other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            if (!(other is SortingAccumulator)) return false;

            SortingAccumulator sorter = (SortingAccumulator) other;
            // TODO Will need the standard comparers to override Equals() appropriately
            return this.Expression.Equals(sorter.Expression) && this.Comparer.Equals(sorter.Comparer);
        }

        protected internal override void Accumulate(IValuedNode value)
        {
            // If the new value exceeds the existing value (according to the comparer then we keep the new value)
            if (this.Comparer.Compare(this.AccumulatedResult, value) > 0)
            {
                this.AccumulatedResult = value;
            }
        }
    }
}
