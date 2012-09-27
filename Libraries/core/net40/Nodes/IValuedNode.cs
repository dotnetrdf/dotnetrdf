/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

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
using System.Linq;
using System.Text;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Interface for Valued Nodes
    /// </summary>
    /// <remarks>
    /// <para>
    /// This interface extends the basic <see cref="INode">INode</see> interface with methods related to turning the lexical value into a strongly typed .Net value.  It is intended primarily for use within SPARQL expression evaluation where we need to do a lot of value conversions and currently waste a lot of effort (and thus performance) doing that.
    /// </para>
    /// </remarks>
    public interface IValuedNode 
        : INode
    {
        /// <summary>
        /// Gets the String value of the Node
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is distinct from <strong>ToString()</strong> because that method will typically include additional information like language specifier/datatype as appropriate whereas this method is used to produce a string as would be produced by applying the <strong>STR()</strong> function from SPARQL
        /// </remarks>
        String AsString();

        /// <summary>
        /// Gets the Long value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Long</exception>
        long AsInteger();

        /// <summary>
        /// Gets the Decimal value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Decimal</exception>
        decimal AsDecimal();

        /// <summary>
        /// Gets the Float value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Float</exception>
        float AsFloat();

        /// <summary>
        /// Gets the Double value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Double</exception>
        double AsDouble();

        /// <summary>
        /// Gets the Boolean value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Boolean</exception>
        bool AsBoolean();

        /// <summary>
        /// Gets the Date Time value of the Node
        /// </summary>
        /// <returns></returns>
        /// <exception cref="RdfQueryException">Thrown if the Node cannot be converted to a Date Time</exception>
        DateTimeOffset AsDateTime();

        /// <summary>
        /// Gets the Time Span value of the Node
        /// </summary>
        /// <returns></returns>
        TimeSpan AsTimeSpan();

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        /// <remarks>
        /// Either String.Empty if no type or the string form of the type URI
        /// </remarks>
        String EffectiveType
        {
            get;
        }

        /// <summary>
        /// Gets the Numeric Type of the Node
        /// </summary>
        SparqlNumericType NumericType
        {
            get;
        }
    }
}
