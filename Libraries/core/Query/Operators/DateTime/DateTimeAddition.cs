using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators.DateTime
{
    public class DateTimeAddition
        : BaseDateTimeOperator
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
            if (ns.Length != 2) throw new RdfQueryException("Incorrect number of arguments");
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply operator when one/more arguments are null");

            DateTimeOffset dateTime = ns[0].AsDateTime();
            TimeSpan addition = ns[1].AsTimeSpan();

            DateTimeOffset result = dateTime.Add(addition);
            return new DateTimeNode(null, result);
        }
    }
}
