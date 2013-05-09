using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.XPath.Cast;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public class SparqlCastBuilderTests
    {
        private SparqlCastBuilder _castCall;
        private SparqlExpression _variable;

        [SetUp]
        public void Setup()
        {
            _variable = new VariableExpression("var");
            _castCall = new SparqlCastBuilder(_variable);
        }

        [Test]
        public void ShouldAllowCastingAsInteger()
        {
            // when
            NumericExpression<int> cast = _castCall.AsInteger();

            // then
            Assert.IsTrue(cast.Expression is IntegerCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsFloat()
        {
            // when
            NumericExpression<float> cast = _castCall.AsFloat();

            // then
            Assert.IsTrue(cast.Expression is FloatCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsDateTime()
        {
            // when
            LiteralExpression cast = _castCall.AsDateTime();

            // then
            Assert.IsTrue(cast.Expression is DateTimeCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsDecimal()
        {
            // when
            NumericExpression<decimal> cast = _castCall.AsDecimal();

            // then
            Assert.IsTrue(cast.Expression is DecimalCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsDouble()
        {
            // when
            NumericExpression<double> cast = _castCall.AsDouble();

            // then
            Assert.IsTrue(cast.Expression is DoubleCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsString()
        {
            // when
            LiteralExpression cast = _castCall.AsString();

            // then
            Assert.IsTrue(cast.Expression is StringCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCastingAsBoolean()
        {
            // when
            BooleanExpression cast = _castCall.AsBoolean();

            // then
            Assert.IsTrue(cast.Expression is BooleanCast);
            Assert.AreSame(_variable.Expression, cast.Expression.Arguments.ElementAt(0));
        }
    }
}