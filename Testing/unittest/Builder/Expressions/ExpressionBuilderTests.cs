using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Query.Builder;
using VDS.RDF.Query.Builder.Expressions;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
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
        public void CanCreateConstantTerms()
        {
            Assert.AreEqual("10 ", Builder.Constant(10).Expression.ToString());
            Assert.AreEqual("10 ", Builder.Constant(10m).Expression.ToString());
            Assert.AreEqual("\"10\"^^<http://www.w3.org/2001/XMLSchema#float>", Builder.Constant(10f).Expression.ToString());
            Assert.AreEqual("\"10\"^^<http://www.w3.org/2001/XMLSchema#double>", Builder.Constant(10d).Expression.ToString());
            Assert.AreEqual("\"2010-10-16T00:00:00\"^^<http://www.w3.org/2001/XMLSchema#dateTime>", Builder.Constant(new DateTime(2010, 10, 16)).Expression.ToString());
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
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

        [TestMethod]
        public void CanCreateStrFunctionWithVariableParameter()
        {
            // given
            var variable = new VariableExpression("x");

            // when
            SimpleLiteralExpression str = Builder.Str(variable);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(variable.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateStrFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new StringExpression("1000");

            // when
            SimpleLiteralExpression str = Builder.Str(literal);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(literal.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateStrFunctionWithIriLiteral()
        {
            // given
            var iri = new IriExpression("urn:some:uri");

            // when
            SimpleLiteralExpression str = Builder.Str(iri);

            // then
            Assert.IsTrue(str.Expression is StrFunction);
            Assert.AreSame(iri.Expression, str.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateLangFunctionWithVariableParameter()
        {
            // given
            var variable = new VariableExpression("x");

            // when
            SimpleLiteralExpression lang = Builder.Lang(variable);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(variable.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateLangFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new StringExpression("1000");

            // when
            SimpleLiteralExpression lang = Builder.Lang(literal);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateDatatypeFunctionWithVariableParameter()
        {
            // given
            VariableExpression literal = new VariableExpression("s");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateDatatypeFunctionWithLiteralParameter()
        {
            // given
            LiteralExpression literal = new StringExpression("1000");

            // when
            IriExpression lang = Builder.Datatype(literal);

            // then
            Assert.IsTrue(lang.Expression is LangFunction);
            Assert.AreSame(literal.Expression, lang.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateBNodeFunctionWithoutParameter()
        {
            // when
            BlankNodeExpression bnode = Builder.BNode();

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.IsFalse(bnode.Expression.Arguments.Any());
        }

        [TestMethod]
        public void CanCreateBNodeFunctionWithSimpleLiteralExpressionParameter()
        {
            // given
            SimpleLiteralExpression expression = new SimpleLiteralExpression(new VariableTerm("S"));

            // when
            BlankNodeExpression bnode = Builder.BNode(expression);

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.AreSame(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
        }

        [TestMethod]
        public void CanCreateBNodeFunctionWithStringLiteralExpressionParameter()
        {
            // given
            var expression = new StringExpression("str");

            // when
            BlankNodeExpression bnode = Builder.BNode(expression);

            // then
            Assert.IsTrue(bnode.Expression is BNodeFunction);
            Assert.AreSame(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
        }
    }
}