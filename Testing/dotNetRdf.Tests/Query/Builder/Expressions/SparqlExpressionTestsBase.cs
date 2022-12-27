/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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
using System.Linq;
using Xunit;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public class SparqlExpressionTestsBase
    {
        protected ISparqlExpression Left;
        protected ISparqlExpression Right;

        protected void AssertExpressionTypeAndCorrectArguments<TExpressionType>(SparqlExpression expression,
                                                                                Action<ISparqlExpression> assertLeftOperand = null,
                                                                                Action<ISparqlExpression> assertRightOperand = null)
        {
            Assert.Equal(typeof(TExpressionType), expression.Expression.GetType());
            if (assertLeftOperand == null)
            {
                Assert.Same(Left, expression.Expression.Arguments.ElementAt(0));
            }
            else
            {
                assertLeftOperand(expression.Expression.Arguments.ElementAt(0));
            }
            if (assertRightOperand == null)
            {
                Assert.Same(Right, expression.Expression.Arguments.ElementAt(1));
            }
            else
            {
                assertRightOperand(expression.Expression.Arguments.ElementAt(1));
            }
        }

        protected void AssertCorrectConstantTerm<TConstant>(ISparqlExpression operand, TConstant value)
        {
            Assert.True(operand is ConstantTerm);
            Assert.Equal(value.ToConstantTerm().ToString(), operand.ToString());
        }
    }
}