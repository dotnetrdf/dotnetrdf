using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    public class SparqlExpressionTestsBase
    {
        protected ISparqlExpression Left;
        protected ISparqlExpression Right;

        protected void AssertExpressionTypeAndCorrectArguments<TExpressionType>(SparqlExpression expression,
                                                                                Action<ISparqlExpression> assertLeftOperand = null,
                                                                                Action<ISparqlExpression> assertRightOperand = null)
        {
            Assert.AreEqual(typeof(TExpressionType), expression.Expression.GetType());
            if (assertLeftOperand == null)
            {
                Assert.AreSame(Left, expression.Expression.Arguments.ElementAt(0));
            }
            else
            {
                assertLeftOperand(expression.Expression.Arguments.ElementAt(0));
            }
            if (assertRightOperand == null)
            {
                Assert.AreSame(Right, expression.Expression.Arguments.ElementAt(1));
            }
            else
            {
                assertRightOperand(expression.Expression.Arguments.ElementAt(1));
            }
        }

        protected void AssertCorrectConstantTerm<TConstant>(ISparqlExpression operand, TConstant value)
        {
            Assert.IsTrue(operand is ConstantTerm);
            Assert.AreEqual(value.ToConstantTerm().ToString(), operand.ToString());
        }
    }
}