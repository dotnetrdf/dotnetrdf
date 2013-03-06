using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Numeric Types for Sparql Numeric Expressions
    /// </summary>
    /// <remarks>All Numeric expressions in Sparql are typed as Integer/Decimal/Double</remarks>
    public enum SparqlNumericType : int
    {
        /// <summary>
        /// Not a Number
        /// </summary>
        NaN = -1,
        /// <summary>
        /// An Integer
        /// </summary>
        Integer = 0,
        /// <summary>
        /// A Decimal
        /// </summary>
        Decimal = 1,
        /// <summary>
        /// A Single precision Floating Point
        /// </summary>
        Float = 2,
        /// <summary>
        /// A Double precision Floating Point
        /// </summary>
        Double = 3
    }
}
