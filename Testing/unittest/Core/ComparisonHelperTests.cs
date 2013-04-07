using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace VDS.RDF.Core
{
    [TestClass]
    public class ComparisonHelperTests : BaseTest
    {
        private Graph _graph;

        [TestInitialize]
        public void Setup()
        {
            _graph = new Graph();
        }

        [TestMethod]
        public void ShouldSuccesfullyCompareDecimalNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const decimal left = 1.4m;
                    const decimal right = 3.55m;
                    Assert.AreEqual(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }

        [TestMethod]
        public void ShouldSuccesfullyCompareFloatNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const float left = 1.4f;
                    const float right = 3.55f;
                    Assert.AreEqual(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }

        [TestMethod]
        public void ShouldSuccesfullyCompareDoubleNodeRegardlessOfCulture()
        {
            foreach (var ci in TestedCultureInfos)
            {
                TestTools.ExecuteWithChangedCulture(ci, () =>
                {
                    const double left = 1.4d;
                    const double right = 3.55d;
                    Assert.AreEqual(-1, ComparisonHelper.CompareLiterals(left.ToLiteral(_graph), right.ToLiteral(_graph)));
                });
            }
        }
    }
}