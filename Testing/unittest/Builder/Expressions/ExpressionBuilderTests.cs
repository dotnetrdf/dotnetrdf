using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    [TestClass]
    public class ExpressionBuilderTests : ExpressionBuilderTestsBase
    {
        [TestMethod]
        public void CanCreateVariableTerm()
        {
            // when
            var variable = Builder.Variable("varName").Expression;

            // then
            Assert.AreEqual("varName", variable.Variables.ElementAt(0));
        }

        [TestMethod]
        public void CanApplyNegationToBooleanExpression()
        {
            // given
            BooleanExpression mail = new BooleanExpression(new VariableTerm("mail"));

            // when
            var negatedBound = Builder.Not(mail).Expression;

            // then
            Assert.IsTrue(negatedBound is NotExpression);
            Assert.AreSame(mail.Expression, negatedBound.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateExistsFunction()
        {
            // given
            Action<IGraphPatternBuilder> graphBuildFunction = gbp => gbp.Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            var exists = Builder.Exists(graphBuildFunction);

            // then
            Assert.IsTrue(exists.Expression is ExistsFunction);
            var graphPatternTerm = (GraphPatternTerm) ((ExistsFunction) exists.Expression).Arguments.ElementAt(0);
            Assert.AreEqual(1, graphPatternTerm.Pattern.TriplePatterns.Count);
            Assert.AreEqual(3, graphPatternTerm.Pattern.Variables.Count());
        }
    }
}