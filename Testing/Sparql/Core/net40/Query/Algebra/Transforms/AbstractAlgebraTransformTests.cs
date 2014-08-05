using System;
using NUnit.Framework;

namespace VDS.RDF.Query.Algebra.Transforms
{
    /// <summary>
    /// Abstract test class for algebra transforms
    /// </summary>
    public abstract class AbstractAlgebraTransformTests
    {
        /// <summary>
        /// Creates an instance of the algebra transform to test
        /// </summary>
        /// <returns>Algebra transform</returns>
        protected abstract IAlgebraTransform CreateInstance();

        /// <summary>
        /// Transforms the algebra using the derived test classes provided transformer obtained by calling the <see cref="CreateInstance"/> method
        /// </summary>
        /// <param name="algebra">Algebra to transform</param>
        /// <returns>Transformed algebra</returns>
        protected IAlgebra Transform(IAlgebra algebra)
        {
            ApplyTransformVisitor transformer = new ApplyTransformVisitor(this.CreateInstance());
            return transformer.Transform(algebra);
        }

        /// <summary>
        /// Checks that the transform under test does not change the given algebra
        /// </summary>
        /// <param name="algebra">Algebra</param>
        protected void CheckUnchanged(IAlgebra algebra)
        {
            CheckTransform(algebra, null);
        }

        /// <summary>
        /// Checks that that the transform has an expected result
        /// </summary>
        /// <param name="algebra">Original Algebra</param>
        /// <param name="expected">Expected Transformed Algebra</param>
        protected void CheckTransform(IAlgebra algebra, IAlgebra expected)
        {
            Console.WriteLine("Original Algebra:");
            Console.WriteLine(algebra.ToString());

            IAlgebra actual = Transform(algebra);
            expected = expected ?? algebra;

            Console.WriteLine("Actual:");
            Console.WriteLine(actual.ToString());
            Console.WriteLine("Expected:");
            Console.WriteLine(expected.ToString());

            Assert.IsTrue(actual.Equals(expected));
        }
    }
}