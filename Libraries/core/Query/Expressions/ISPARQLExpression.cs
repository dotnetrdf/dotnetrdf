/*

Copyright Robert Vesse 2009-10
rvesse@vdesign-studios.com

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

using System;
using System.Collections.Generic;

namespace VDS.RDF.Query.Expressions
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

    /// <summary>
    /// Interface for SPARQL Expression Terms that can be used in Expression Trees while evaluating Sparql Queries
    /// </summary>
    public interface ISparqlExpression
    {
        /// <summary>
        /// Gets the Value of the SPARQL Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        INode Value(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Effective Boolean Value of the SPARQL Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        bool EffectiveBooleanValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets an enumeration of all the Variables used in an expression
        /// </summary>
        IEnumerable<String> Variables
        {
            get;
        }
    }

    /// <summary>
    /// Interface for SPARQL Expression Terms that are expected to provide a Numeric value
    /// </summary>
    public interface ISparqlNumericExpression : ISparqlExpression
    {
        /// <summary>
        /// Gets the Numeric Type of a SPARQL Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        SparqlNumericType NumericType(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        object NumericValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as an Integer as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        long IntegerValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as a Decimal as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        /// <returns></returns>
        decimal DecimalValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as a Float as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        float FloatValue(SparqlEvaluationContext context, int bindingID);

        /// <summary>
        /// Gets the Numeric Value of a Sparql Expression as a Double as evaluated for a given Binding
        /// </summary>
        /// <param name="context">Evaluation Context</param>
        /// <param name="bindingID">Binding ID</param>
        double DoubleValue(SparqlEvaluationContext context, int bindingID);
    }

    /// <summary>
    /// Marker interface for Expression classes which represent Binary Operators
    /// </summary>
    /// <remarks>
    /// Used as a marker so that the ToString() implementations can appropriately bracket expressions with binary operators so that the precedence of the expression is clear
    /// </remarks>
    public interface IBinaryOperator
    {

    }

}