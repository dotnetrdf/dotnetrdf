using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    public class DivisionOperator
        : BaseNumericOperator
    {
        public override SparqlOperatorType Operator
        {
            get
            {
                return SparqlOperatorType.Divide;
            }
        }

        public override Nodes.IValuedNode Apply(params Nodes.IValuedNode[] ns)
        {
            if (ns.Any(n => n == null)) throw new RdfQueryException("Cannot apply division when any arguments are null");

            SparqlNumericType type = (SparqlNumericType)ns.Max(n => (int)n.NumericType);

            try
            {
                switch (type)
                {
                    case SparqlNumericType.Integer:
                    case SparqlNumericType.Decimal:
                        //For Division Integers are treated as decimals
                        decimal d = this.Divide(ns.Select(n => n.AsDecimal()));
                        if (Decimal.Floor(d).Equals(d) && d >= Int64.MinValue && d <= Int64.MaxValue)
                        {
                            return new LongNode(null, Convert.ToInt64(d));
                        }
                        return new DecimalNode(null, d);
                    case SparqlNumericType.Float:
                        return new FloatNode(null, this.Divide(ns.Select(n => n.AsFloat())));
                    case SparqlNumericType.Double:
                        return new DoubleNode(null, this.Divide(ns.Select(n => n.AsDouble())));
                    default:
                        throw new RdfQueryException("Cannot evalute an Arithmetic Expression when the Numeric Type of the expression cannot be determined");
                }
            }
            catch (DivideByZeroException)
            {
                throw new RdfQueryException("Cannot evaluate a Division Expression where the divisor is Zero");
            }
        }

        private decimal Divide(IEnumerable<decimal> ls)
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
                    total /= l;
                }
            }
            return total;
        }

        private float Divide(IEnumerable<float> ls)
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
                    total /= l;
                }
            }
            return total;
        }

        private double Divide(IEnumerable<double> ls)
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
                    total /= l;
                }
            }
            return total;
        }
    }
}
