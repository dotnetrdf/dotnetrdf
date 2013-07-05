/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2013 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System.Linq;
using NUnit.Framework;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    public partial class ExpressionBuilderTests
    {
        [Test]
        public void ShouldAllowCreatingRegexFunctionWithVariableAndStringParameters()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            var regex = Builder.Regex(var, "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(2, regex.Arguments.Count());
            Assert.AreSame(var.Expression, regex.Arguments.ElementAt(0));
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithTwoVariableParameters()
        {
            // given
            VariableExpression text = new VariableExpression("mail");
            VariableExpression pattern = new VariableExpression("pattern");

            // when
            var regex = Builder.Regex(text, pattern).Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(2, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithLiteralAndStringParameters()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());

            // when
            var regex = Builder.Regex(text, "@gmail.com$").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(2, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithLiteralAndVariableParameters()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());
            VariableExpression pattern = new VariableExpression("pattern");

            // when
            var regex = Builder.Regex(text, pattern).Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(2, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithTwoLiteralParameters()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());
            LiteralExpression pattern = new LiteralExpression("@gmail.com$".ToSimpleLiteral());

            // when
            var regex = Builder.Regex(text, pattern).Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(2, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithVariableAndStringParametersAndStringFlags()
        {
            // given
            VariableExpression var = new VariableExpression("mail");

            // when
            var regex = Builder.Regex(var, "@gmail.com$", "i").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(3, regex.Arguments.Count());
            Assert.AreSame(var.Expression, regex.Arguments.ElementAt(0));
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(2) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithTwoVariableParametersAndStringFlags()
        {
            // given
            VariableExpression text = new VariableExpression("mail");
            VariableExpression pattern = new VariableExpression("pattern");

            // when
            var regex = Builder.Regex(text, pattern, "i").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(3, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
            Assert.IsTrue(regex.Arguments.ElementAt(2) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithLiteralAndStringParametersAndStringFlags()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());

            // when
            var regex = Builder.Regex(text, "@gmail.com$", "i").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(3, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.IsTrue(regex.Arguments.ElementAt(1) is ConstantTerm);
            Assert.IsTrue(regex.Arguments.ElementAt(2) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithLiteralAndVariableParametersAndStringFlags()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());
            VariableExpression pattern = new VariableExpression("pattern");

            // when
            var regex = Builder.Regex(text, pattern, "i").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(3, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
            Assert.IsTrue(regex.Arguments.ElementAt(2) is ConstantTerm);
        }

        [Test]
        public void ShouldAllowCreatingRegexFunctionWithTwoLiteralParametersAndStringFlags()
        {
            // given
            LiteralExpression text = new LiteralExpression("mail".ToSimpleLiteral());
            LiteralExpression pattern = new LiteralExpression("@gmail.com$".ToSimpleLiteral());

            // when
            var regex = Builder.Regex(text, pattern, "i").Expression;

            // then
            Assert.IsTrue(regex is RegexFunction);
            Assert.AreEqual(3, regex.Arguments.Count());
            Assert.AreSame(text.Expression, regex.Arguments.ElementAt(0));
            Assert.AreSame(pattern.Expression, regex.Arguments.ElementAt(1));
            Assert.IsTrue(regex.Arguments.ElementAt(2) is ConstantTerm);
        }

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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

        [Test]
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