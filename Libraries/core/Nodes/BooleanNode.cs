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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing boolean values
    /// </summary>
    public class BooleanNode
        : LiteralNode, IValuedNode
    {
        private bool _value;

        /// <summary>
        /// Creates a new boolean valued node
        /// </summary>
        /// <param name="g">Graph the node belong to</param>
        /// <param name="value">Boolean Value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public BooleanNode(IGraph g, bool value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeBoolean))
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new boolean valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Boolean Value</param>
        public BooleanNode(IGraph g, bool value)
            : this(g, value, value.ToString().ToLower()) { }

        /// <summary>
        /// Gets the string value of the boolean
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to integers
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to decimals
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to floats
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to doubles
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Gets the boolean value
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            return this._value;
        }

        /// <summary>
        /// Throws an error as booleans cannot be cast to date times
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot cast Boolean to other types");
        }

        /// <summary>
        /// Gets the numeric type of the node
        /// </summary>
        public SparqlNumericType NumericType
        {
            get 
            { 
                return SparqlNumericType.NaN; 
            }
        }
    }
}
