/*
// <copyright>
// dotNetRDF is free and open source software licensed under the MIT License
// -------------------------------------------------------------------------
// 
// Copyright (c) 2009-2017 dotNetRDF Project (http://dotnetrdf.org/)
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished
// to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// </copyright>
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
            _value = value;
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
                return Convert.ToInt64(_value);
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
                return Convert.ToDecimal(_value);
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
                return Convert.ToSingle(_value);
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
                return Convert.ToDouble(_value);
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
            _value = value;
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
                return Convert.ToInt64(_value);
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
                return Convert.ToDecimal(_value);
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
                return Convert.ToSingle(_value);
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
                return Convert.ToDouble(_value);
            }
            catch
            {
                throw new RdfQueryException("Unable to upcast byte to double");
            }
        }
    }
}
