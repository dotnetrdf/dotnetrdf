/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2015 dotNetRDF Project (dotnetrdf-develop@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using VDS.RDF.Graphs;
using VDS.RDF.Parsing;
using VDS.RDF.Specifications;
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
        /// <param name="value">Byte value</param>
        /// <param name="lexicalValue">Lexical value</param>
        public ByteNode(byte value, String lexicalValue)
            : base(lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedByte), EffectiveNumericType.Integer)
        {
            this._value = value;
        }

        /// <summary>
        /// Creates a new byte valued node
        /// </summary>
        /// <param name="value">Byte value</param>
        public ByteNode(byte value)
            : this(value, value.ToString()) { }

        
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
                throw new NodeValueException("Unable to upcast byte to integer");
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
                throw new NodeValueException("Unable to upcast byte to decimal");
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
                throw new NodeValueException("Unable to upcast byte to float");
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
                throw new NodeValueException("Unable to upcast byte to double");
            }
        }
    }
}
