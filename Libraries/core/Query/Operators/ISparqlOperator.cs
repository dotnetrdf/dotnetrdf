using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VDS.RDF.Nodes;

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// Interface which represents an operator in SPARQL e.g. +
    /// </summary>
    public interface ISparqlOperator
    {
        /// <summary>
        /// Gets the Operator this is an implementation of
        /// </summary>
        SparqlOperatorType Operator
        {
            get;
        }

        /// <summary>
        /// Gets whether the operator can be applied to the given inputs
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        bool IsApplicable(params IValuedNode[] ns);

        /// <summary>
        /// Applies the operator to the given inputs
        /// </summary>
        /// <param name="ns">Inputs</param>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if an error occurs in applying the operator</exception>
        IValuedNode Apply(params IValuedNode[] ns);
    }
}
