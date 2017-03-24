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

using System.Linq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Functions.Sparql.Set;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a SPARQL expression which is not an aggregate
    /// </summary>
#pragma warning disable 660, 661
    public abstract class SparqlExpression : PrimaryExpression<ISparqlExpression>
#pragma warning restore 660,661
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SparqlExpression"/> class.
        /// </summary>
        /// <param name="expression">The expression.</param>
        protected SparqlExpression(ISparqlExpression expression) : base(expression)
        {
        }

        /// <summary>
        /// Creates a call to the IN function
        /// </summary>
        /// <param name="expressions">the list of SPARQL expressions</param>
        public BooleanExpression In(params SparqlExpression[] expressions)
        {
            var inFunction = new InFunction(Expression, expressions.Select(v => v.Expression));
            return new BooleanExpression(inFunction);
        }

#pragma warning disable 1591
        public static BooleanExpression operator ==(SparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.Expression));
        }

        public static BooleanExpression operator !=(SparqlExpression left, SparqlExpression right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.Expression));
        }
#pragma warning restore 1591

        /// <summary>
        /// Creates a greater than operator usage
        /// </summary>
        protected static BooleanExpression Gt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanExpression(left, right));
        }

        /// <summary>
        /// Creates a less than operator usage
        /// </summary>
        protected static BooleanExpression Lt(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanExpression(left, right));
        }

        /// <summary>
        /// Creates a greater than or equal operator usage
        /// </summary>
        protected static BooleanExpression Ge(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new GreaterThanOrEqualToExpression(left, right));
        }

        /// <summary>
        /// Creates a less than or equal operator usage
        /// </summary>
        protected static BooleanExpression Le(ISparqlExpression left, ISparqlExpression right)
        {
            return new BooleanExpression(new LessThanOrEqualToExpression(left, right));
        }
    }
}