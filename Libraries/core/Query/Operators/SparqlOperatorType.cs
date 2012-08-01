using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Query.Operators
{
    /// <summary>
    /// Possible SPARQL operand types
    /// </summary>
    public enum SparqlOperatorType
    {
        /// <summary>
        /// Addition
        /// </summary>
        Add,
        /// <summary>
        /// Subtraction
        /// </summary>
        Subtract,
        /// <summary>
        /// Multiplication
        /// </summary>
        Multiply,
        /// <summary>
        /// Division
        /// </summary>
        Divide
    }
}
