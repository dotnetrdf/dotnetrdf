using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators.DateTime
{
    public class TimeSpanSubtraction
        : BaseTimeSpanOperator
    {
        public override SparqlOperatorType Operator
        {
            get 
            {
                return SparqlOperatorType.Subtract;
            }
        }

        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply operator when one/more arguments are null");

            return new TimeSpanNode(null, this.Subtract(ns.Select(n => n.AsTimeSpan())));
        }

        private TimeSpan Subtract(IEnumerable<TimeSpan> ts)
        {
            bool first = true;
            TimeSpan total = TimeSpan.Zero;
            foreach (TimeSpan t in ts)
            {
                if (first)
                {
                    total = t;
                    first = false;
                }
                else
                {
                    total -= t;
                }
            }
            return total;
        }
    }
}
