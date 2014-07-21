using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Comparer which applies a sequence of sort conditions
    /// </summary>
    public class SortConditionApplicator
        : IComparer<ISolution>
    {
        public SortConditionApplicator(IEnumerable<ISortCondition> conditions)
        {
            this.SortConditions = conditions.ToList();
        }

        private IList<ISortCondition> SortConditions { get; set; }

        public int Compare(ISolution x, ISolution y)
        {
            int c = 0;
            for (int i = 0; i < this.SortConditions.Count; i++)
            {
                c = this.SortConditions[i].Compare(x, y);
                if (c != 0) break;
            }
            return c;
        }
    }
}
