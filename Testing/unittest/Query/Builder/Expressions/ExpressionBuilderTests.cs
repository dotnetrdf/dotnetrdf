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

using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions
{
    [TestFixture]
    public partial class ExpressionBuilderTests
    {
        private ExpressionBuilder Builder { get; set; }
        private Mock<INamespaceMapper> _prefixes;

        [SetUp]
        public void Setup()
        {
            _prefixes = new Mock<INamespaceMapper>(MockBehavior.Strict);
            Builder = new ExpressionBuilder(_prefixes.Object);
        }

        [Test]
        public void CanCreateVariableTerm()
        {
            // when
            var variable = Builder.Variable("varName").Expression;

            // then
            Assert.AreEqual("varName", variable.Variables.ElementAt(0));
        }

        [Test]
        public void CanCreateConstantTerms()
        {
            Assert.AreEqual("10 ", Builder.Constant(10).Expression.ToString());
            Assert.AreEqual("10 ", Builder.Constant(10m).Expression.ToString());
            Assert.AreEqual("\"10\"^^<http://www.w3.org/2001/XMLSchema#float>", Builder.Constant(10f).Expression.ToString());
            Assert.AreEqual("\"10\"^^<http://www.w3.org/2001/XMLSchema#double>", Builder.Constant(10d).Expression.ToString());
            Assert.AreEqual("\"2010-10-16T00:00:00.000000\"^^<http://www.w3.org/2001/XMLSchema#dateTime>", Builder.Constant(new DateTime(2010, 10, 16)).Expression.ToString());
        }

        [Test]
        public void CanApplyNegationToBooleanExpression()
        {
            // given
            BooleanExpression mail = new BooleanExpression(new VariableTerm("mail"));

            // when
            var negatedBound = (!mail).Expression;

            // then
            Assert.IsTrue(negatedBound is NotExpression);
            Assert.AreSame(mail.Expression, negatedBound.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateExistsFunction()
        {
            // given
            Action<IGraphPatternBuilder> graphBuildFunction = gbp => gbp.Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

            // when
            var exists = Builder.Exists(graphBuildFunction);

            // then
            Assert.IsTrue(exists.Expression is ExistsFunction);
            var graphPatternTerm = (GraphPatternTerm)((ExistsFunction)exists.Expression).Arguments.ElementAt(0);
            Assert.AreEqual(1, graphPatternTerm.Pattern.TriplePatterns.Count);
            Assert.AreEqual(3, graphPatternTerm.Pattern.Variables.Count());
        }

        [Test]
        public void CanCreateSameTermFunction()
        {
            // given
            SparqlExpression left = new VariableExpression("x");
            SparqlExpression right = new NumericExpression<int>(10);

            // when
            BooleanExpression sameTerm = Builder.SameTerm(left, right);

            // then
            Assert.IsTrue(sameTerm.Expression is SameTermFunction);
            Assert.AreSame(left.Expression, sameTerm.Expression.Arguments.ElementAt(0));
            Assert.AreSame(right.Expression, sameTerm.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void CanCreateSameTermFunctionUsingVariableNameForFirstParameter()
        {
            // given
            SparqlExpression left = new VariableExpression("x");
            SparqlExpression right = new NumericExpression<int>(10);

            // when
            BooleanExpression sameTerm = Builder.SameTerm("x", right);

            // then
            Assert.IsTrue(sameTerm.Expression is SameTermFunction);
            Assert.AreEqual(left.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(0).ToString());
            Assert.AreSame(right.Expression, sameTerm.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void CanCreateSameTermFunctionUsingVariableNameForSecondParameter()
        {
            // given
            SparqlExpression right = new VariableExpression("x");
            SparqlExpression left = new NumericExpression<int>(10);

            // when
            BooleanExpression sameTerm = Builder.SameTerm(left, "x");

            // then
            Assert.IsTrue(sameTerm.Expression is SameTermFunction);
            Assert.AreSame(left.Expression, sameTerm.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(right.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(1).ToString());
        }

        [Test]
        public void CanCreateSameTermFunctionUsingVariableNameForBothParameter()
        {
            // given
            SparqlExpression right = new VariableExpression("x");
            SparqlExpression left = new VariableExpression("y");

            // when
            BooleanExpression sameTerm = Builder.SameTerm("y", "x");

            // then
            Assert.IsTrue(sameTerm.Expression is SameTermFunction);
            Assert.AreEqual(left.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(0).ToString());
            Assert.AreEqual(right.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(1).ToString());
        }

        [Test]
        public void CanCreateIsIRIFunction()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression sameTerm = Builder.IsIRI(variable);

            // then
            Assert.IsTrue(sameTerm.Expression is IsIriFunction);
            Assert.AreSame(variable.Expression, sameTerm.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateIsIRIFunctionUsingVariableName()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isIRI = Builder.IsIRI("x");

            // then
            Assert.IsTrue(isIRI.Expression is IsIriFunction);
            Assert.AreEqual(variable.Expression.ToString(), isIRI.Expression.Arguments.ElementAt(0).ToString());
        }

        [Test]
        public void CanCreateIsBlankFunction()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isBlank = Builder.IsBlank(variable);

            // then
            Assert.IsTrue(isBlank.Expression is IsBlankFunction);
            Assert.AreSame(variable.Expression, isBlank.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateIsBlankFunctionUsingVariableName()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isBlank = Builder.IsBlank("x");

            // then
            Assert.IsTrue(isBlank.Expression is IsBlankFunction);
            Assert.AreEqual(variable.Expression.ToString(), isBlank.Expression.Arguments.ElementAt(0).ToString());
        }

        [Test]
        public void CanCreateIsLiteralFunction()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isLiteral = Builder.IsLiteral(variable);

            // then
            Assert.IsTrue(isLiteral.Expression is IsLiteralFunction);
            Assert.AreSame(variable.Expression, isLiteral.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateIsLiteralFunctionUsingVariableName()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isLiteral = Builder.IsLiteral("x");

            // then
            Assert.IsTrue(isLiteral.Expression is IsLiteralFunction);
            Assert.AreEqual(variable.Expression.ToString(), isLiteral.Expression.Arguments.ElementAt(0).ToString());
        }

        [Test]
        public void CanCreateIsNumericFunction()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isNumeric = Builder.IsNumeric(variable);

            // then
            Assert.IsTrue(isNumeric.Expression is IsNumericFunction);
            Assert.AreSame(variable.Expression, isNumeric.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateIsNumericFunctionUsingVariableName()
        {
            // given
            SparqlExpression variable = new VariableExpression("x");

            // when
            BooleanExpression isNumeric = Builder.IsNumeric("x");

            // then
            Assert.IsTrue(isNumeric.Expression is IsNumericFunction);
            Assert.AreEqual(variable.Expression.ToString(), isNumeric.Expression.Arguments.ElementAt(0).ToString());
        }

        [Test]
        public void CanCreateStrFunctionWithVariableParameter()
        {
            // given
            var variable = new VariableExpression("x");

            // when
            LiteralExpression str = Builder.Str(variable);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(variable.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateStrFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new TypedLiteralExpression<string>("1000");

            // when
            LiteralExpression str = Builder.Str(literal);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(literal.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateStrFunctionWithIriLiteral()
        {
            // given
            var iri = new IriExpression(new Uri("urn:some:uri"));

            // when
            LiteralExpression str = Builder.Str(iri);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(iri.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateLangFunctionWithVariableParameter()
        {
            // given
            var variable = new VariableExpression("x");

            // when
            LiteralExpression lang = Builder.Lang(variable);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(variable.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateLangFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new TypedLiteralExpression<string>("1000");

            // when
            LiteralExpression lang = Builder.Lang(literal);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateDatatypeFunctionWithVariableParameter()
        {
            // given
            VariableExpression literal = new VariableExpression("s");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is DataType11Function);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateDatatypeFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new TypedLiteralExpression<string>("1000");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is DataType11Function);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateOldDatatypeFunctionWithVariableParameter()
        {
            // given
            VariableExpression literal = new VariableExpression("s");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is DataTypeFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateOldDatatypeFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new TypedLiteralExpression<string>("1000");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is DataTypeFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateBNodeFunctionWithoutParameter()
        {
            // when
            BlankNodeExpression bnode = Builder.BNode();

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.IsFalse(bnode.Expression.Arguments.Any());
        }

        [Test]
        public void CanCreateBNodeFunctionWithSimpleLiteralExpressionParameter()
        {
            // given
            LiteralExpression expression = new LiteralExpression(new VariableTerm("S"));

            // when
            BlankNodeExpression bnode = Builder.BNode(expression);

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.AreSame(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void CanCreateBNodeFunctionWithStringLiteralExpressionParameter()
        {
            // given
            var expression = new TypedLiteralExpression<string>("str");

            // when
            BlankNodeExpression bnode = Builder.BNode(expression);

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.AreSame(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndIriExpressionParameters()
        {
            // given
            LiteralExpression expression = new LiteralExpression(new VariableTerm("S"));
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt(expression, iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithStringAnrIriExpressionParameters()
        {
            // given
            var expression = "literal".ToConstantTerm();
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt("literal", iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreEqual(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndUriParameters()
        {
            // given
            LiteralExpression expression = new LiteralExpression(new VariableTerm("S"));
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt(expression, new Uri("http://example.com"));

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithVariableExpressionAndUriParameters()
        {
            // given
            var expression = new VariableExpression("literalVar");
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt(expression, new Uri("http://example.com"));

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreEqual(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithStringAndUriParameters()
        {
            // given
            var expression = "literal".ToConstantTerm();
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt("literal", new Uri("http://example.com"));

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreEqual(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
            Assert.AreEqual(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithStringAndVariableParameters()
        {
            // given
            var expression = "literal".ToConstantTerm();
            var iriExpression = new VariableExpression("var");

            // when
            LiteralExpression literal = Builder.StrDt("literal", iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreEqual(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndVariableExpressionParameters()
        {
            // given
            LiteralExpression expression = new LiteralExpression(new VariableTerm("S"));
            var iriExpression = new VariableExpression("var");

            // when
            LiteralExpression literal = Builder.StrDt(expression, iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithTwoVariableParameters()
        {
            // given
            var expression = new VariableExpression("literalVar");
            var iriExpression = new VariableExpression("var");

            // when
            LiteralExpression literal = Builder.StrDt(expression, iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrdtFunctionWithVariableAndIriExpressionParameters()
        {
            // given
            var expression = new VariableExpression("literalVar");
            var iriExpression = new IriExpression(new Uri("http://example.com"));

            // when
            LiteralExpression literal = Builder.StrDt(expression, iriExpression);

            // then
            Assert.IsTrue(literal.Expression is StrDtFunction);
            Assert.AreSame(expression.Expression, literal.Expression.Arguments.ElementAt(0));
            Assert.AreSame(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
        }

        [Test]
        public void ShouldAllowCreatingStrUuidFucntionCall()
        {
            // when
            LiteralExpression uuid = Builder.StrUUID();

            // then
            Assert.IsTrue(uuid.Expression is StrUUIDFunction);
        }

        [Test]
        public void ShouldAllowCreatingUuidFucntionCall()
        {
            // when
            IriExpression uuid = Builder.UUID();

            // then
            Assert.IsTrue(uuid.Expression is UUIDFunction);
        }

        [Test]
        public void ShouldAllowCastingAsXsdInt()
        {
            // given
            SparqlExpression expression = new VariableExpression("variable");

            // when
            SparqlCastBuilder cast = Builder.Cast(expression);

            // then
            Assert.IsNotNull(cast);
        }

        [Test]
        public void CanBuildSumAggregateGivenVariableName()
        {
            // when
            var sum = Builder.Sum("s");

            // then
            Assert.That(sum.Expression.ToString(), Is.EqualTo("SUM(?s)"));
        }

        [Test]
        public void CanBuildSumAggregateGivenAnExpression()
        {
            // when
            var sum = Builder.Sum(Builder.StrLen(Builder.Variable("x")));

            // then
            Assert.That(sum.Expression.ToString(), Is.EqualTo("SUM(STRLEN(?x))"));
        }
    }
}