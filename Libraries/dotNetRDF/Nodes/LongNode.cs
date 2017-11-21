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
    /// A Valued Node with a Long value
    /// </summary>
    public class LongNode
        : NumericNode
    {
        private long _value;

        /// <summary>
        /// Creates a new long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public LongNode(IGraph g, long value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeInteger)) { }

        /// <summary>
        /// Creates a new long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        public LongNode(IGraph g, long value, String lexicalValue, Uri datatype)
            : base(g, lexicalValue, datatype, SparqlNumericType.Integer)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Long value</param>
        public LongNode(IGraph g, long value)
            : this(g, value, value.ToString()) { }

        /// <summary>
        /// Gets the long value
        /// </summary>
        /// <returns></returns>
        public override long AsInteger()
        {
            return _value;
        }

        /// <summary>
        /// Gets the decimal value of the long
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
                throw new RdfQueryException("Unable to upcast Long to Double");
            }
        }

        /// <summary>
        /// Gets the float value of the long
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
                throw new RdfQueryException("Unable to upcast Long to Float");
            }
        }

        /// <summary>
        /// Gets the double value of the long
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
                throw new RdfQueryException("Unable to upcast Long to Double");
            }
        }
    }

    /// <summary>
    /// A Valued Node with a unsigned long value
    /// </summary>
    public class UnsignedLongNode
        : NumericNode
    {
        private ulong _value;

        /// <summary>
        /// Creates a new unsigned long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Unsigned Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        public UnsignedLongNode(IGraph g, ulong value, String lexicalValue)
            : this(g, value, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeUnsignedInt)) { }

        /// <summary>
        /// Creates a new unsigned long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Unsigned Long value</param>
        /// <param name="lexicalValue">Lexical Value</param>
        /// <param name="datatype">Datatype URI</param>
        public UnsignedLongNode(IGraph g, ulong value, String lexicalValue, Uri datatype)
            : base(g, lexicalValue, datatype, SparqlNumericType.Integer)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new usigned long valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Unsigned Long value</param>
        public UnsignedLongNode(IGraph g, ulong value)
            : this(g, value, value.ToString()) { }

        /// <summary>
        /// Gets the long value of the ulong
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
                throw new RdfQueryException("Unable to downcast unsigned Long to long");
            }
        }

        /// <summary>
        /// Gets the decimal value of the ulong
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
                throw new RdfQueryException("Unable to upcast Long to Double");
            }
        }

        /// <summary>
        /// Gets the float value of the ulong
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
                throw new RdfQueryException("Unable to upcast Long to Float");
            }
        }

        /// <summary>
        /// Gets the double value of the ulong
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
                throw new RdfQueryException("Unable to upcast Long to Double");
            }
        }
    }
}
