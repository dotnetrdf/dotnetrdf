using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    public class MultiplicationOperator
        : BaseNumericOperator
    {
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Multiply;
            }
        }

        public override IValuedNode Apply(params IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply multiplication when any arguments are null");

            SparqlNumericType type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

            switch (type)
            {
                case SparqlNumericType.Integer:
                    return new LongNode(null, this.Multiply(ns.Select(n => n.AsInteger())));
                case SparqlNumericType.Decimal:
                    return new DecimalNode(null, this.Multiply(ns.Select(n => n.AsDecimal())));
                case SparqlNumericType.Float:
                    return new FloatNode(null, this.Multiply(ns.Select(n => n.AsFloat())));
                case SparqlNumericType.Double:
                    return new DoubleNode(null, this.Multiply(ns.Select(n => n.AsDouble())));
                default:
                    throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
            }
        }

        private long Multiply(IEnumerable<long> ls)
        {
            bool first = true;
            long total = 0;
            foreach (long l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total *= l;
                }
            }
            return total;
        }

        private decimal Multiply(IEnumerable<decimal> ls)
        {
            bool first = true;
            decimal total = 0;
            foreach (decimal l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total *= l;
                }
            }
            return total;
        }

        private float Multiply(IEnumerable<float> ls)
        {
            bool first = true;
            float total = 0;
            foreach (float l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total *= l;
                }
            }
            return total;
        }

        private double Multiply(IEnumerable<double> ls)
        {
            bool first = true;
            double total = 0;
            foreach (double l in ls)
            {
                if (first)
                {
                    total = l;
                    first = false;
                }
                else
                {
                    total *= l;
                }
            }
            return total;
        }
    }
}
