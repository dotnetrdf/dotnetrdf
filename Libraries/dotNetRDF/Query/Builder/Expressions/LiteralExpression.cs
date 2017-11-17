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

using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Comparison;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    /// <summary>
    /// Represents a literal expression
    /// </summary>
#pragma warning disable 660,661
    public class LiteralExpression : RdfTermExpression
#pragma warning restore 660,661
    {
        /// <summary>
        /// Wraps the <paramref name="expression"/> as a literal expression
        /// </summary>
        public LiteralExpression(ISparqlExpression expression) : base(expression)
        {
        }

#pragma warning disable 1591
        public static BooleanExpression operator ==(LiteralExpression left, string right)
        {
            return new BooleanExpression(new EqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator !=(LiteralExpression left, string right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.Expression, right.ToConstantTerm()));
        }

        public static BooleanExpression operator ==(string left, LiteralExpression right)
        {
            return new BooleanExpression(new EqualsExpression(left.ToConstantTerm(), right.Expression));
        }

        public static BooleanExpression operator !=(string left, LiteralExpression right)
        {
            return new BooleanExpression(new NotEqualsExpression(left.ToConstantTerm(), right.Expression));
#pragma warning restore 1591
        }

        /// <summary>
        /// Returns the constant value of this expression formatted as a simple literal expression
        /// </summary>
        /// <returns>A new LiteralExpression whose value is the constant value of this epression</returns>
        public LiteralExpression ToSimpleLiteral()
        {
            ConstantTerm constant = (ConstantTerm) Expression;
            return new LiteralExpression(constant.Node.AsString().ToSimpleLiteral());
        }
    }
}