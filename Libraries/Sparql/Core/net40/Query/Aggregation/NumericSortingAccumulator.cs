using System.Collections.Generic;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Aggregation
{
    public class NumericSortingAccumulator
        : SortingAccumulator
    {
        public NumericSortingAccumulator(IExpression expr, IComparer<IValuedNode> comparer) 
            : this(expr, comparer, null) {}

        public NumericSortingAccumulator(IExpression expr, IComparer<IValuedNode> comparer, IValuedNode initialValue) : base(expr, comparer, initialValue) {}

        protected internal override void Accumulate(IValuedNode value)
        {
            // Ignore non-numeric values
            if (value == null || value.NodeType != NodeType.Literal || value.NumericType == EffectiveNumericType.NaN) return;

            // Only accumulate numeric values
            base.Accumulate(value);
        }
    }
}
