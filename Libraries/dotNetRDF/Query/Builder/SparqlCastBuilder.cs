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

using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;

namespace VDS.RDF.Query.Builder
{
    /// <summary>
    /// Provides methods for casting expressions to XPath types
    /// </summary>
    public sealed class SparqlCastBuilder
    {
        private readonly SparqlExpression _castedExpression;

        internal SparqlCastBuilder(SparqlExpression castedExpression)
        {
            _castedExpression = castedExpression;
        }

        /// <summary>
        /// Creates a cast to xsd:integer
        /// </summary>
        public NumericExpression<int> AsInteger()
        {
            return new NumericExpression<int>(new IntegerCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:double
        /// </summary>
        public NumericExpression<double> AsDouble()
        {
            return new NumericExpression<double>(new DoubleCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:decimal
        /// </summary>
        public NumericExpression<decimal> AsDecimal()
        {
            return new NumericExpression<decimal>(new DecimalCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:dateTime
        /// </summary>
        public LiteralExpression AsDateTime()
        {
            return new LiteralExpression(new DateTimeCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:float
        /// </summary>
        public NumericExpression<float> AsFloat()
        {
            return new NumericExpression<float>(new FloatCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:boolean
        /// </summary>
        public BooleanExpression AsBoolean()
        {
            return new BooleanExpression(new BooleanCast(_castedExpression.Expression));
        }

        /// <summary>
        /// Creates a cast to xsd:string
        /// </summary>
        public LiteralExpression AsString()
        {
            return new LiteralExpression(new StringCast(_castedExpression.Expression));
        }
    }
}