using System;

namespace VDS.RDF.Query.Engine.Sorting
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

        public int Compare(ISet x, ISet y)
        {
            int c = this._condition.Compare(x, y);
            // Reverse sort order if inner condition is an ascending sort
            // If it is already a descending sort leave as is
            return this._condition.IsAscending ? c*-1 : c;
        }
    }
}
