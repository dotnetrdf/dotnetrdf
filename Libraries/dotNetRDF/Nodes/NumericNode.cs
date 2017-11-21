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
        protected NumericNode(IGraph g, String value, Uri datatype, SparqlNumericType numType)
            : base(g, value, datatype) 
        {
            _numType = numType;
        }

        /// <summary>
        /// Gets the string value of the node
        /// </summary>
        /// <returns></returns>
        public string AsString()
        {
            return Value;
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
            switch (_numType)
            {
                case SparqlNumericType.Integer:
                    return AsInteger() != 0;
                case SparqlNumericType.Decimal:
                    return AsDecimal() != Decimal.Zero;
                case SparqlNumericType.Float:
                    return AsFloat() != 0.0f && !float.IsNaN(AsFloat());
                case SparqlNumericType.Double:
                    return AsDouble() != 0.0d && !double.IsNaN(AsDouble());
                default:
                    return SparqlSpecsHelper.EffectiveBooleanValue(this);
            }
        }

        /// <summary>
        /// Throws an error as numerics cannot be converted to date times
        /// </summary>
        /// <returns></returns>
        public DateTime AsDateTime()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Date Times");
        }

        /// <summary>
        /// Throws an error as numerics cannot be converted to date times
        /// </summary>
        /// <returns></returns>
        public DateTimeOffset AsDateTimeOffset()
        {
            throw new RdfQueryException("Numeric Types cannot be converted into Date Times");
        }

        /// <summary>
        /// Throws an error as numerics cannot be cast to a time span
        /// </summary>
        /// <returns></returns>
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
                return DataType.AbsoluteUri;
            }
        }

        /// <summary>
        /// Gets the numeric type of the node
        /// </summary>
        public SparqlNumericType NumericType
        {
            get
            {
                return _numType;
            }
        }
    }
}
