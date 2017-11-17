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
    /// A Valued Node representing decimal nodes
    /// </summary>
    public class DecimalNode
        : NumericNode
    {
        private decimal _value;

        /// <summary>
        /// Creates a new decimal valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Decimal value</param>
        /// <param name="lexicalValue">Lexical value</param>
        public DecimalNode(IGraph g, decimal value, String lexicalValue)
            : base(g, lexicalValue, UriFactory.Create(XmlSpecsHelper.XmlSchemaDataTypeDecimal), SparqlNumericType.Decimal)
        {
            _value = value;
        }

        /// <summary>
        /// Creates a new decimal valued node
        /// </summary>
        /// <param name="g">Graph the node belongs to</param>
        /// <param name="value">Decimal value</param>
        public DecimalNode(IGraph g, decimal value)
            : this(g, value, value.ToString()) { }

        /// <summary>
        /// Gets the integer value of the decimal
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
                throw new RdfQueryException("Unable to downcast Decimal to Long");
            }
        }

        /// <summary>
        /// Gets the decimal value
        /// </summary>
        /// <returns></returns>
        public override decimal AsDecimal()
        {
            return _value;
        }

        /// <summary>
        /// Gets the float value of the decimal
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
                throw new RdfQueryException("Unable to cast Decimal to Float");
            }
        }

        /// <summary>
        /// Gets the double value of the decimal
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
                throw new RdfQueryException("Unable to cast Decimal to Double");
            }
        }
    }
}
