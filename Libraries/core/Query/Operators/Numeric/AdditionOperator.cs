using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    public class AdditionOperator
        : BaseNumericOperator
    {
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Add;
            }
        }

        public override Nodes.IValuedNode Apply(params Nodes.IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply addition when any arguments are null");

            SparqlNumericType type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, ns.Select(n => n.AsInteger()).Sum());
                case SparqlNumericType.Decimal:
                    return new DecimalNode(null, ns.Select(n => n.AsDecimal()).Sum());
                case SparqlNumericType.Float:
                    return new FloatNode(null, ns.Select(n => n.AsFloat()).Sum());
                case SparqlNumericType.Double:
                    return new DoubleNode(null, ns.Select(n => n.AsDouble()).Sum());
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the operation cannot be determined");
            }
        }
    }
}
