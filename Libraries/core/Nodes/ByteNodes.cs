/*

Copyright Robert Vesse 2009-11
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
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Query.Expressions;

namespace VDS.RDF.Nodes
{
    /// <summary>
    /// Valued node representing a byte (8-bit unsigned integer)
    /// </summary>
    public class ByteNode
        : NumericNode
    {
        private byte _value;

        /// <summary>
        /// Creates a new byte valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Byte value</param>
        /// <param name="lexicalValue">Lexical value</param>
        public ByteNode(IGraph g, byte value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte), SparqlNumericType.Integer)
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new byte valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Byte value</param>
        public ByteNode(IGraph g, byte value)
            : this(g, value, value.ToString()) { }

        
        /// <summary>
        /// Gets the integer value of the byte
        /// </summary>
        /// <returns></returns>
        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to integer");
            }
        }

        /// <summary>
        /// Gets the decimal value of the byte
        /// </summary>
        /// <returns></returns>
        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to decimal");
            }
        }

        /// <summary>
        /// Gets the float value of the byte
        /// </summary>
        /// <returns></returns>
        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to float");
            }
        }

        /// <summary>
        /// Gets the float value of the double
        /// </summary>
        /// <returns></returns>
        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to double");
            }
        }
    }

    /// <summary>
    /// Value node representing a signed byte (8-bit signed integer)
    /// </summary>
    public class SignedByteNode
        : NumericNode
    {
        private sbyte _value;

        /// <summary>
        /// Creates a new signed byte node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Signed Byte value</param>
        /// <param name="lexicalValue">Lexical value</param>
        public SignedByteNode(IGraph g, sbyte value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeByte), SparqlNumericType.Integer)
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new signed byte node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Signed Byte value</param>
        public SignedByteNode(IGraph g, sbyte value)
            : this(g, value, value.ToString()) { }

        /// <summary>
        /// Gets the integer value of the signed byte
        /// </summary>
        /// <returns></returns>
        public override long AsInteger()
        {
            try
            {
                return Convert.ToInt64(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to integer");
            }
        }

        /// <summary>
        /// Gets the decimal value of the signed byte
        /// </summary>
        /// <returns></returns>
        public override decimal AsDecimal()
        {
            try
            {
                return Convert.ToDecimal(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to decimal");
            }
        }

        /// <summary>
        /// Gets the float value of the signed byte
        /// </summary>
        /// <returns></returns>
        public override float AsFloat()
        {
            try
            {
                return Convert.ToSingle(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to float");
            }
        }

        /// <summary>
        /// Gets the double value of the signed byte
        /// </summary>
        /// <returns></returns>
        public override double AsDouble()
        {
            try
            {
                return Convert.ToDouble(this._value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to double");
            }
        }
    }
}
