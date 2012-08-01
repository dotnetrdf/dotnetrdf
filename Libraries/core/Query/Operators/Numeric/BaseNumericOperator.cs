using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    public abstract class BaseNumericOperator
        : ISparqlOperator
    {
        public bool IsApplicable(params IValuedNode[] ns)
        {
            return ns.All(n => n.NumericType != SparqlNumericType.NaN);
        }

        public abstract IValuedNode Apply(params IValuedNode[] ns);
    }
}
