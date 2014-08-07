using System;
using System.Collections.Generic;
using System.Linq;
using VDS.RDF.Query.Engine;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Sorting
{
    /// <summary>
    /// Comparer which applies a sequence of sort conditions
    /// </summary>
    public class SortConditionApplicator
        : IComparer<ISolution>
    {
        public SortConditionApplicator(IEnumerable<ISortCondition> conditions, IExpressionContext context)
        {
            if (conditions == null) throw new ArgumentNullException("conditions");
            if (context == null) throw new ArgumentNullException("context");
            this.SortConditions = conditions.ToList();
            this.SortComparers = this.SortConditions.Select(c => c.CreateComparer(context)).ToList();
        }

        private IList<ISortCondition> SortConditions { get; set; }

        private IList<IComparer<ISolution>> SortComparers { get; set; } 

        public int Compare(ISolution x, ISolution y)
        {
            int c = 0;
            for (int i = 0; i < this.SortComparers.Count; i++)
            {
                c = this.SortComparers[i].Compare(x, y);
                if (c != 0) break;
            }
            return c;
        }
    }
}
