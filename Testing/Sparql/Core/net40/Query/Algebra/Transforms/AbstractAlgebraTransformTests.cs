using System;
using NUnit.Framework;

namespace VDS.RDF.Query.Algebra.Transforms
{
    public abstract class AbstractAlgebraTransformTests
    {
        /// <summary>
        /// Creates an instance of the algebra transform to test
        /// </summary>
        /// <returns>Algebra transform</returns>
        protected abstract IAlgebraTransform CreateInstance();

        protected IAlgebra Transform(IAlgebra algebra)
        {
            ApplyTransformVisitor transformer = new ApplyTransformVisitor(this.CreateInstance());
            return transformer.Transform(algebra);
        }

        protected void CheckUnchanged(IAlgebra algebra)
        {
            CheckTransform(algebra, null);
        }

        protected void CheckTransform(IAlgebra algebra, IAlgebra expected)
        {
            Console.WriteLine("Original Algebra:");
            Console.WriteLine(algebra.ToString());
            Console.WriteLine();

            IAlgebra actual = Transform(algebra);
            expected = expected ?? algebra;

            Console.WriteLine("Actual:");
            Console.WriteLine(actual.ToString());
            Console.WriteLine();
            Console.WriteLine("Expected:");
            Console.WriteLine(expected.ToString());
            Console.WriteLine();

            Assert.IsTrue(actual.Equals(expected));
        }
    }
}