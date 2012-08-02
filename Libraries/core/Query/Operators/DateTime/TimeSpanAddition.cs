using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators.DateTime
{
    public class TimeSpanAddition
        : BaseTimeSpanOperator
    {
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Add;
            }
        }

        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply operator when one/more arguments are null");

            return new TimeSpanNode(null, this.Add(ns.Select(n => n.AsTimeSpan())));
        }

        private TimeSpan Add(IEnumerable<TimeSpan> ts)
        {
            TimeSpan total = TimeSpan.Zero;
            foreach (TimeSpan t in ts)
            {
                total += t;
            }
            return total;
        }
    }
}
