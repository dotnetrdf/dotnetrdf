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
    /// A Valued Node with a numeric value
    /// </summary>
    public abstract class NumericNode
        : LiteralNode, IValuedNode
    {
        private SparqlNumericType _numType = SparqlNumericType.NaN;

        /// <summary>
        /// Creates a new numeric valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        /// <param name="numType">SPARQL Numeric Type</param>
        public NumericNode(IGraph g, String value, Uri datatype, SparqlNumericType numType)
            : base(g, value, datatype) 
        {
            this._numType = numType;
        }

        /// <summary>
        /// Gets the string value of the node
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return this.Value;
        }

        /// <summary>
        /// Gets the integer value
        /// </summary>
        /// <returns></returns>
        public abstract long AsInteger();

        /// <summary>
        /// Gets the decimal value
        /// </summary>
        /// <returns></returns>
        public abstract decimal AsDecimal();

        /// <summary>
        /// Gets the float value
        /// </summary>
        /// <returns></returns>
        public abstract float AsFloat();

        /// <summary>
        /// Gets the double value
        /// </summary>
        /// <returns></returns>
        public abstract double AsDouble();

        /// <summary>
        /// Gets the boolean value
        /// </summary>
        /// <returns></returns>
        public bool AsBoolean()
        {
            switch (this._numType)
            {
                case SparqlNumericType.Integer:
                    return this.AsInteger() != 0;
                case SparqlNumericType.Decimal:
                    return this.AsDecimal() != Decimal.Zero;
                case SparqlNumericType.Float:
                    return this.AsFloat() != 0.0f && this.AsFloat() != Single.NaN;
                case SparqlNumericType.Double:
                    return this.AsDouble() != 0.0d && this.AsDouble() != Double.NaN;
                default:
                    return SparqlSpecsHelper.EffectiveBooleanValue(this);
            }
        }

        /// <summary>
        /// Throws an error as numerics cannot be converted to date times
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTime()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Date Times");
        }

        public TimeSpan AsTimeSpan()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Time spans");
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
                return this._numType;
            }
        }
    }
}
