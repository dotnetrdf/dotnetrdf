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
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued node whose value is a string or can only be converted to a string
    /// </summary>
    public class StringNode
        : LiteralNode, IValuedNode
    {
        /// <summary>
        /// Creates a new String Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="value">String value</param>
        /// <param name="datatype">Datatype URI</param>
        public StringNode(IGraph g, String value, Uri datatype)
            : base(g, value, datatype) { }

        /// <summary>
        /// Creates a new String Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="value">String value</param>
        /// <param name="lang">Language Specifier</param>
        public StringNode(IGraph g, String value, String lang)
            : base(g, value, lang) { }

        /// <summary>
        /// Creates a new String Node
        /// </summary>
        /// <param name="g">Graph the Node belongs to</param>
        /// <param name="value">String value</param>
        public StringNode(IGraph g, String value)
            : base(g, value) { }

        #region IValuedNode Members

        /// <summary>
        /// Gets the string value
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to an integer
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a decimal
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a float
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a double
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Gets the boolean value of the string
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            return SparqlSpecsHelper.EffectiveBooleanValue(this);
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Gets the numeric type of the expression
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            {
                return SparqlNumericType.NaN;
            }
        }

        #endregion
    }
}
