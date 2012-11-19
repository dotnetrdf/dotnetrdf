using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Test.Builder.Expressions
{
    public partial class ExpressionBuilderTests
    {
        [TestMethod]
        public void CanCreateRegexExpressionWithVariableAndString()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            var regex = Builder.Regex(var, "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.IsTrue(regex.Arguments.ElementAt(0) is VariableTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToStrlenFunctionWithVariableParameter()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            NumericExpression<int> strlen = Builder.StrLen(var);

            // then
            Assert.IsTrue(strlen.Expression is StrLenFunction);
            Assert.AreSame(var.Expression, strlen.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToStrlenFunctionWithStringLiteralParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");

            // when
            NumericExpression<int> strlen = Builder.StrLen(literal);

            // then
            Assert.IsTrue(strlen.Expression is StrLenFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndNumericExpressionParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndIntegerParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndNumericExpressionParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndIntegerParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithTwoVariableParameters()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new VariableExpression("startFrom");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithLiteralExpressionAndVariableParameters()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new VariableExpression("startFrom");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndNumericExpressionAndIntegerParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndIntegerAndIntegerParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndNumericExpressionAndIntegerParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndIntegerAndIntegerParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithTwoVariableAndIntegerParameters()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new VariableExpression("startFrom");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithLiteralExpressionAndVariableAndIntegerParameters()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new VariableExpression("startFrom");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, 5);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreEqual(new NumericExpression<int>(5).Expression.ToString(), strlen.Expression.Arguments.ElementAt(2).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndNumericExpressionAndIntegerExpressionParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndIntegerAndIntegerExpressionParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndNumericExpressionAndIntegerExpressionParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndIntegerAndIntegerExpressionParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithTwoVariableAndIntegerExpressionParameters()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new VariableExpression("startFrom");
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithLiteralExpressionAndVariableAndIntegerExpressionParameters()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new VariableExpression("startFrom");
            var length = new NumericExpression<int>(5);

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndNumericExpressionAndVariableExpressionParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithStringLiteralAndIntegerAndVariableExpressionParameter()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndNumericExpressionAndVariableExpressionParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithVariableAndIntegerAndVariableExpressionParameter()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new NumericExpression<int>(5);
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, 5, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(startLocation.Expression.ToString(), strlen.Expression.Arguments.ElementAt(1).ToString());
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithTwoVariableAndVariableExpressionParameters()
        {
            // given
            var literal = new VariableExpression("mail");
            var startLocation = new VariableExpression("startFrom");
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToSubstrFunctionWithLiteralExpressionAndVariableAndVariableExpressionParameters()
        {
            // given
            var literal = new TypedLiteralExpression<string>("mail");
            var startLocation = new VariableExpression("startFrom");
            var length = new VariableExpression("len");

            // when
            TypedLiteralExpression<string> strlen = Builder.Substr(literal, startLocation, length);

            // then
            Assert.IsTrue(strlen.Expression is SubStrFunction);
            Assert.AreSame(literal.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(startLocation.Expression, strlen.Expression.Arguments.ElementAt(1));
            Assert.AreSame(length.Expression, strlen.Expression.Arguments.ElementAt(2));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithLiteralExpressionAndStringParameters()
        {
            // given
            var languageTag = new TypedLiteralExpression<string>("title");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, "fr");

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual("\"fr\"", strlen.Expression.Arguments.ElementAt(1).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithVariableAndStringParameters()
        {
            // given
            var languageTag = new VariableExpression("title");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, "fr");

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreEqual("\"fr\"", strlen.Expression.Arguments.ElementAt(1).ToString());
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithLiteralExpressionAndLiteralExpressionParameters()
        {
            // given
            LiteralExpression languageTag = new TypedLiteralExpression<string>("title");
            LiteralExpression languageRange = new TypedLiteralExpression<string>("*");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, languageRange);

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(languageRange.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithVariableAndLiteralExpressionParameters()
        {
            // given
            var languageTag = new VariableExpression("title");
            LiteralExpression languageRange = new TypedLiteralExpression<string>("*");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, languageRange);

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(languageRange.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithLiteralExpressionAndVariableParameters()
        {
            // given
            LiteralExpression languageTag = new TypedLiteralExpression<string>("title");
            var languageRange = new VariableExpression("range");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, languageRange);

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(languageRange.Expression, strlen.Expression.Arguments.ElementAt(1));
        }

        [TestMethod]
        public void ShouldAllowCreatingCallToLangMatchesFunctionWithTwoVariableParameters()
        {
            // given
            var languageTag = new VariableExpression("title");
            VariableExpression languageRange = new VariableExpression("range");

            // when
            BooleanExpression strlen = Builder.LangMatches(languageTag, languageRange);

            // then
            Assert.IsTrue(strlen.Expression is LangMatchesFunction);
            Assert.AreSame(languageTag.Expression, strlen.Expression.Arguments.ElementAt(0));
            Assert.AreSame(languageRange.Expression, strlen.Expression.Arguments.ElementAt(1));
        }
    }
}