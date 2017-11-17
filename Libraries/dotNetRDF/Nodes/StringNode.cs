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
            return Value;
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
        public DateTime AsDateTime()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a date time
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Cannot cast this literal node to a type");
        }

        /// <summary>
        /// Throws an error as the string cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
        public TimeSpan AsTimeSpan()
        {
            throw new RdfQueryException("Cannot case this literal node to a type");
        }

        /// <summary>
        /// Gets the URI of the datatype this valued node represents as a String
        /// </summary>
        public String EffectiveType
        {
            get
            {
                return (DataType != null ? DataType.AbsoluteUri : String.Empty);
            }
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
