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
using System.Xml;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued Node representing a Time Span value
    /// </summary>
    public class TimeSpanNode
        : LiteralNode, IValuedNode
    {
        private TimeSpan _value;

        public TimeSpanNode(IGraph g, TimeSpan value)
            : this(g, value, XmlConvert.ToString(value), UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

        public TimeSpanNode(IGraph g, TimeSpan value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDuration)) { }

        public TimeSpanNode(IGraph g, TimeSpan value, String lexicalValue, Uri dtUri)
            : base(g, lexicalValue, dtUri)
        {
            this._value = value;
        }

        /// <summary>
        /// Gets the date time value as a string
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to integers
        /// </summary>
        /// <returns></returns>
        public long AsInteger()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to decimals
        /// </summary>
        /// <returns></returns>
        public decimal AsDecimal()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to floats
        /// </summary>
        /// <returns></returns>
        public float AsFloat()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to doubles
        /// </summary>
        /// <returns></returns>
        public double AsDouble()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Throws an error as date times cannot be converted to booleans
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        /// <summary>
        /// Gets the date time value of the node
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Cannot convert Time Spans to other types");
        }

        public TimeSpan AsTimeSpan()
        {
            return this._value;
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return this.DataType.AbsoluteUri;
            }
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
