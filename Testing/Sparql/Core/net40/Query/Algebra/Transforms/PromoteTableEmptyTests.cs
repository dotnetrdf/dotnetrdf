using NUnit.Framework;

namespace VDS.RDF.Query.Algebra.Transforms
{
    [TestFixture]
    public class PromoteTableEmptyTests
        : AbstractAlgebraTransformTests
    {
        protected override IAlgebraTransform CreateInstance()
        {
            return new PromoteTableEmpty();
        }

        [Test]
        public void PromoteTableEmptyJoin1()
        {
            IAlgebra lhs = Table.CreateEmpty();
            IAlgebra rhs = Table.CreateUnit();

            IAlgebra join = Join.CreateDirect(lhs, rhs);
            Assert.IsInstanceOf(typeof(Join), join);

            CheckTransform(join, lhs);
        }

        [Test]
        public void PromoteTableEmptyJoin2()
        {
            IAlgebra lhs = Table.CreateUnit();
            IAlgebra rhs = Table.CreateEmpty();

            IAlgebra join = Join.CreateDirect(lhs, rhs);
            Assert.IsInstanceOf(typeof(Join), join);

            CheckTransform(join, rhs);
        }

        [Test]
        public void PromoteTableEmptyJoin3()
        {
            IAlgebra lhs = Table.CreateEmpty();
            IAlgebra rhs = Table.CreateEmpty();

            IAlgebra join = Join.CreateDirect(lhs, rhs);
            Assert.IsInstanceOf(typeof(Join), join);

            CheckTransform(join, lhs);
        }

        [Test]
        public void PromoteTableEmptyJoin4()
        {
            IAlgebra lhs = Table.CreateUnit();
            IAlgebra rhs = Join.CreateDirect(Table.CreateUnit(), Table.CreateEmpty());

            IAlgebra join = Join.CreateDirect(lhs, rhs);
            Assert.IsInstanceOf(typeof(Join), join);

            CheckTransform(join, Table.CreateEmpty());
        }
    }
}
