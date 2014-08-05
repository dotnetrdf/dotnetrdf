using System;
using VDS.RDF.Query.Engine;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Query.Sorting
{
    public class DescendingSortCondition
        : ISortCondition
    {
        public readonly ISortCondition _condition;

        public DescendingSortCondition(ISortCondition condition)
        {
            if (condition == null) throw new ArgumentNullException("condition");
            this._condition = condition;
        }

        public bool IsAscending
        {
            get { return false; }
        }

        public override string ToString()
        {
            return ToString(new AlgebraFormatter());
        }

        public string ToString(INodeFormatter formatter)
        {
            if (formatter == null) throw new ArgumentNullException("formatter");
            return this._condition.IsAscending ? String.Format("(desc {0})", this._condition.ToString(formatter)) : this._condition.ToString(formatter);
        }

        public int Compare(ISolution x, ISolution y)
        {
            int c = this._condition.Compare(x, y);
            // Reverse sort order if inner condition is an ascending sort
            // If it is already a descending sort leave as is
            return this._condition.IsAscending ? c*-1 : c;
        }

        public bool Equals(ISortCondition other)
        {
            if (ReferenceEquals(this, other)) return true;
            if (other == null) return false;
            return !other.IsAscending && this._condition.Equals(other);
        }
    }
}
