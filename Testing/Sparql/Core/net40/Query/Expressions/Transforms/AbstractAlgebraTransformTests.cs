using System;
using NUnit.Framework;

namespace VDS.RDF.Query.Expressions.Transforms
{
    /// <summary>
    /// Abstract test class for algebra transforms
    /// </summary>
    public abstract class AbstractExpressionTransformTests
    {
        /// <summary>
        /// Creates an instance of the expression transform to test
        /// </summary>
        /// <returns>Expression transform</returns>
        protected abstract IExpressionTransform CreateInstance();

        /// <summary>
        /// Transforms the expression using the derived test classes provided transformer obtained by calling the <see cref="CreateInstance"/> method
        /// </summary>
        /// <param name="expression">Expression to transform</param>
        /// <returns>Transformed expression</returns>
        protected IExpression Transform(IExpression expression)
        {
            ApplyExpressionTransformVisitor transformer = new ApplyExpressionTransformVisitor(this.CreateInstance());
            return transformer.Transform(expression);
        }

        /// <summary>
        /// Checks that the transform under test does not change the given expression
        /// </summary>
        /// <param name="expression">Expression</param>
        protected void CheckUnchanged(IExpression expression)
        {
            CheckTransform(expression, null);
        }

        /// <summary>
        /// Checks that that the transform has an expected result
        /// </summary>
        /// <param name="expression">Original Expression</param>
        /// <param name="expected">Expected Transformed Expression</param>
        protected void CheckTransform(IExpression expression, IExpression expected)
        {
            Console.WriteLine("Original Expression:");
            Console.WriteLine(expression.ToString());

            IExpression actual = Transform(expression);
            expected = expected ?? expression;

            Console.WriteLine("Actual:");
            Console.WriteLine(actual.ToString());
            Console.WriteLine("Expected:");
            Console.WriteLine(expected.ToString());

            Assert.IsTrue(actual.Equals(expected));
        }
    }
}