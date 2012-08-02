using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Query.Operators.Numeric
{
    /// <summary>
    /// Abstract base class for numeric operators
    /// </summary>
    public abstract class BaseNumericOperator
        : ISparqlOperator
    {
        /// <summary>
        /// Gets the operator this implementation represents
        /// </summary>
        public abstract SparqlOperatorType Operator
        {
            get;
        }

        /// <summary>
        /// Operator is applicable if at least one input and all inputs are numeric
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        public bool IsApplicable(params IValuedNode[] ns)
        {
            return ns.Length > 0 && ns.All(n => n != null && n.NumericType != SparqlNumericType.NaN);
        }

        /// <summary>
        /// Applies the operator
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        public abstract IValuedNode Apply(params IValuedNode[] ns);
    }
}
