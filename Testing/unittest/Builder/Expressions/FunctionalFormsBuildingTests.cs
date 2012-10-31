using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class FunctionalFormsBuildingTests : ExpressionBuilderTestsBase
    {

        [TestMethod]
        public void CanCreateBoundFunctionUsingVariableTerm()
        {
            // when
            BooleanExpression bound = Builder.Bound("person");

            // then
            Assert.IsTrue(bound.Expression is BoundFunction);
            Assert.IsTrue(bound.Expression.Arguments.ElementAt(0) is VariableTerm);
        }

        [TestMethod]
        public void CanCreateBoundFunctionUsingVariableName()
        {
            // given
            var variableTerm = new VariableExpression("person");

            // when
            BooleanExpression bound = Builder.Bound(variableTerm);

            // then
            Assert.IsTrue(bound.Expression is BoundFunction);
            Assert.AreSame(variableTerm.Expression, bound.Expression.Arguments.ElementAt(0));
        }
    }
}