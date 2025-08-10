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
using Xunit;
using Moq;
using VDS.RDF.Query.Expressions.Conditional;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Functions.Sparql.Constructor;
using VDS.RDF.Query.Expressions.Functions.Sparql.String;
using VDS.RDF.Query.Expressions.Primary;

namespace VDS.RDF.Query.Builder.Expressions;


public partial class ExpressionBuilderTests
{
    private ExpressionBuilder Builder { get; set; }
    private Mock<INamespaceMapper> _prefixes;

    public ExpressionBuilderTests()
    {
        _prefixes = new Mock<INamespaceMapper>(MockBehavior.Strict);
        Builder = new ExpressionBuilder(_prefixes.Object);
    }

    [Fact]
    public void CanCreateVariableTerm()
    {
        // when
        var variable = Builder.Variable("varName").Expression;

        // then
        Assert.Equal("varName", variable.Variables.ElementAt(0));
    }

    [Fact]
    public void CanCreateConstantTerms()
    {
        Assert.Equal("10 ", Builder.Constant(10).Expression.ToString());
        Assert.Equal("10 ", Builder.Constant(10m).Expression.ToString());
        Assert.Equal("\"10\"^^<http://www.w3.org/2001/XMLSchema#float>", Builder.Constant(10f).Expression.ToString());
        Assert.Equal("\"10\"^^<http://www.w3.org/2001/XMLSchema#double>", Builder.Constant(10d).Expression.ToString());
        Assert.Equal("\"2010-10-16T00:00:00.000000\"^^<http://www.w3.org/2001/XMLSchema#dateTime>", Builder.Constant(new DateTime(2010, 10, 16)).Expression.ToString());
    }

    [Fact]
    public void CanApplyNegationToBooleanExpression()
    {
        // given
        var mail = new BooleanExpression(new VariableTerm("mail"));

        // when
        var negatedBound = (!mail).Expression;

        // then
        Assert.True(negatedBound is NotExpression);
        Assert.Same(mail.Expression, negatedBound.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateExistsFunction()
    {
        // given
        Action<IGraphPatternBuilder> graphBuildFunction = gbp => gbp.Where(tpb => tpb.Subject("s").Predicate("p").Object("o"));

        // when
        var exists = Builder.Exists(graphBuildFunction);

        // then
        Assert.True(exists.Expression is ExistsFunction);
        var graphPatternTerm = (GraphPatternTerm)((ExistsFunction)exists.Expression).Arguments.ElementAt(0);
        Assert.Single(graphPatternTerm.Pattern.TriplePatterns);
        Assert.Equal(3, graphPatternTerm.Pattern.Variables.Count());
    }

    [Fact]
    public void CanCreateSameTermFunction()
    {
        // given
        SparqlExpression left = new VariableExpression("x");
        SparqlExpression right = new NumericExpression<int>(10);

        // when
        BooleanExpression sameTerm = Builder.SameTerm(left, right);

        // then
        Assert.True(sameTerm.Expression is SameTermFunction);
        Assert.Same(left.Expression, sameTerm.Expression.Arguments.ElementAt(0));
        Assert.Same(right.Expression, sameTerm.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void CanCreateSameTermFunctionUsingVariableNameForFirstParameter()
    {
        // given
        SparqlExpression left = new VariableExpression("x");
        SparqlExpression right = new NumericExpression<int>(10);

        // when
        BooleanExpression sameTerm = Builder.SameTerm("x", right);

        // then
        Assert.True(sameTerm.Expression is SameTermFunction);
        Assert.Equal(left.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(0).ToString());
        Assert.Same(right.Expression, sameTerm.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void CanCreateSameTermFunctionUsingVariableNameForSecondParameter()
    {
        // given
        SparqlExpression right = new VariableExpression("x");
        SparqlExpression left = new NumericExpression<int>(10);

        // when
        BooleanExpression sameTerm = Builder.SameTerm(left, "x");

        // then
        Assert.True(sameTerm.Expression is SameTermFunction);
        Assert.Same(left.Expression, sameTerm.Expression.Arguments.ElementAt(0));
        Assert.Equal(right.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(1).ToString());
    }

    [Fact]
    public void CanCreateSameTermFunctionUsingVariableNameForBothParameter()
    {
        // given
        SparqlExpression right = new VariableExpression("x");
        SparqlExpression left = new VariableExpression("y");

        // when
        BooleanExpression sameTerm = Builder.SameTerm("y", "x");

        // then
        Assert.True(sameTerm.Expression is SameTermFunction);
        Assert.Equal(left.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(0).ToString());
        Assert.Equal(right.Expression.ToString(), sameTerm.Expression.Arguments.ElementAt(1).ToString());
    }

    [Fact]
    public void CanCreateIsIRIFunction()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression sameTerm = Builder.IsIRI(variable);

        // then
        Assert.True(sameTerm.Expression is IsIriFunction);
        Assert.Same(variable.Expression, sameTerm.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateIsIRIFunctionUsingVariableName()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isIRI = Builder.IsIRI("x");

        // then
        Assert.True(isIRI.Expression is IsIriFunction);
        Assert.Equal(variable.Expression.ToString(), isIRI.Expression.Arguments.ElementAt(0).ToString());
    }

    [Fact]
    public void CanCreateIsBlankFunction()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isBlank = Builder.IsBlank(variable);

        // then
        Assert.True(isBlank.Expression is IsBlankFunction);
        Assert.Same(variable.Expression, isBlank.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateIsBlankFunctionUsingVariableName()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isBlank = Builder.IsBlank("x");

        // then
        Assert.True(isBlank.Expression is IsBlankFunction);
        Assert.Equal(variable.Expression.ToString(), isBlank.Expression.Arguments.ElementAt(0).ToString());
    }

    [Fact]
    public void CanCreateIsLiteralFunction()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isLiteral = Builder.IsLiteral(variable);

        // then
        Assert.True(isLiteral.Expression is IsLiteralFunction);
        Assert.Same(variable.Expression, isLiteral.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateIsLiteralFunctionUsingVariableName()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isLiteral = Builder.IsLiteral("x");

        // then
        Assert.True(isLiteral.Expression is IsLiteralFunction);
        Assert.Equal(variable.Expression.ToString(), isLiteral.Expression.Arguments.ElementAt(0).ToString());
    }

    [Fact]
    public void CanCreateIsNumericFunction()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isNumeric = Builder.IsNumeric(variable);

        // then
        Assert.True(isNumeric.Expression is IsNumericFunction);
        Assert.Same(variable.Expression, isNumeric.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateIsNumericFunctionUsingVariableName()
    {
        // given
        SparqlExpression variable = new VariableExpression("x");

        // when
        BooleanExpression isNumeric = Builder.IsNumeric("x");

        // then
        Assert.True(isNumeric.Expression is IsNumericFunction);
        Assert.Equal(variable.Expression.ToString(), isNumeric.Expression.Arguments.ElementAt(0).ToString());
    }

    [Fact]
    public void CanCreateStrFunctionWithVariableParameter()
    {
        // given
        var variable = new VariableExpression("x");

        // when
        LiteralExpression str = Builder.Str(variable);

        // then
        Assert.True(str.Expression is StrFunction);
        Assert.Same(variable.Expression, str.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateStrFunctionWithLiteralParameter()
    {
        // given
        LiteralExpression literal = new TypedLiteralExpression<string>("1000");

        // when
        LiteralExpression str = Builder.Str(literal);

        // then
        Assert.True(str.Expression is StrFunction);
        Assert.Same(literal.Expression, str.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateStrFunctionWithIriLiteral()
    {
        // given
        var iri = new IriExpression(new Uri("urn:some:uri"));

        // when
        LiteralExpression str = Builder.Str(iri);

        // then
        Assert.True(str.Expression is StrFunction);
        Assert.Same(iri.Expression, str.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateLangFunctionWithVariableParameter()
    {
        // given
        var variable = new VariableExpression("x");

        // when
        LiteralExpression lang = Builder.Lang(variable);

        // then
        Assert.True(lang.Expression is LangFunction);
        Assert.Same(variable.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateLangFunctionWithLiteralParameter()
    {
        // given
        LiteralExpression literal = new TypedLiteralExpression<string>("1000");

        // when
        LiteralExpression lang = Builder.Lang(literal);

        // then
        Assert.True(lang.Expression is LangFunction);
        Assert.Same(literal.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateDatatypeFunctionWithVariableParameter()
    {
        // given
        var literal = new VariableExpression("s");

        // when
        IriExpression lang = Builder.Datatype(literal);

        // then
        Assert.True(lang.Expression is DataType11Function);
        Assert.Same(literal.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateDatatypeFunctionWithLiteralParameter()
    {
        // given
        LiteralExpression literal = new TypedLiteralExpression<string>("1000");

        // when
        IriExpression lang = Builder.Datatype(literal);

        // then
        Assert.True(lang.Expression is DataType11Function);
        Assert.Same(literal.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateOldDatatypeFunctionWithVariableParameter()
    {
        // given
        var literal = new VariableExpression("s");

        // when
        IriExpression lang = Builder.Datatype(literal);

        // then
        Assert.True(lang.Expression is DataTypeFunction);
        Assert.Same(literal.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateOldDatatypeFunctionWithLiteralParameter()
    {
        // given
        LiteralExpression literal = new TypedLiteralExpression<string>("1000");

        // when
        IriExpression lang = Builder.Datatype(literal);

        // then
        Assert.True(lang.Expression is DataTypeFunction);
        Assert.Same(literal.Expression, lang.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateBNodeFunctionWithoutParameter()
    {
        // when
        BlankNodeExpression bnode = Builder.BNode();

        // then
        Assert.True(bnode.Expression is BNodeFunction);
        Assert.False(bnode.Expression.Arguments.Any());
    }

    [Fact]
    public void CanCreateBNodeFunctionWithSimpleLiteralExpressionParameter()
    {
        // given
        var expression = new LiteralExpression(new VariableTerm("S"));

        // when
        BlankNodeExpression bnode = Builder.BNode(expression);

        // then
        Assert.True(bnode.Expression is BNodeFunction);
        Assert.Same(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void CanCreateBNodeFunctionWithStringLiteralExpressionParameter()
    {
        // given
        var expression = new TypedLiteralExpression<string>("str");

        // when
        BlankNodeExpression bnode = Builder.BNode(expression);

        // then
        Assert.True(bnode.Expression is BNodeFunction);
        Assert.Same(expression.Expression, bnode.Expression.Arguments.ElementAt(0));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndIriExpressionParameters()
    {
        // given
        var expression = new LiteralExpression(new VariableTerm("S"));
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt(expression, iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithStringAnrIriExpressionParameters()
    {
        // given
        var expression = "literal".ToConstantTerm();
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt("literal", iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Equal(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndUriParameters()
    {
        // given
        var expression = new LiteralExpression(new VariableTerm("S"));
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt(expression, new Uri("http://example.com"));

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Equal(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithVariableExpressionAndUriParameters()
    {
        // given
        var expression = new VariableExpression("literalVar");
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt(expression, new Uri("http://example.com"));

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Equal(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithStringAndUriParameters()
    {
        // given
        var expression = "literal".ToConstantTerm();
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt("literal", new Uri("http://example.com"));

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Equal(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
        Assert.Equal(iriExpression.Expression.ToString(), literal.Expression.Arguments.ElementAt(1).ToString());
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithStringAndVariableParameters()
    {
        // given
        var expression = "literal".ToConstantTerm();
        var iriExpression = new VariableExpression("var");

        // when
        LiteralExpression literal = Builder.StrDt("literal", iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Equal(expression.ToString(), literal.Expression.Arguments.ElementAt(0).ToString());
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithLiteralExpressionAndVariableExpressionParameters()
    {
        // given
        var expression = new LiteralExpression(new VariableTerm("S"));
        var iriExpression = new VariableExpression("var");

        // when
        LiteralExpression literal = Builder.StrDt(expression, iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithTwoVariableParameters()
    {
        // given
        var expression = new VariableExpression("literalVar");
        var iriExpression = new VariableExpression("var");

        // when
        LiteralExpression literal = Builder.StrDt(expression, iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrdtFunctionWithVariableAndIriExpressionParameters()
    {
        // given
        var expression = new VariableExpression("literalVar");
        var iriExpression = new IriExpression(new Uri("http://example.com"));

        // when
        LiteralExpression literal = Builder.StrDt(expression, iriExpression);

        // then
        Assert.True(literal.Expression is StrDtFunction);
        Assert.Same(expression.Expression, literal.Expression.Arguments.ElementAt(0));
        Assert.Same(iriExpression.Expression, literal.Expression.Arguments.ElementAt(1));
    }

    [Fact]
    public void ShouldAllowCreatingStrUuidFucntionCall()
    {
        // when
        LiteralExpression uuid = Builder.StrUUID();

        // then
        Assert.True(uuid.Expression is StrUUIDFunction);
    }

    [Fact]
    public void ShouldAllowCreatingUuidFucntionCall()
    {
        // when
        IriExpression uuid = Builder.UUID();

        // then
        Assert.True(uuid.Expression is UUIDFunction);
    }

    [Fact]
    public void ShouldAllowCastingAsXsdInt()
    {
        // given
        SparqlExpression expression = new VariableExpression("variable");

        // when
        SparqlCastBuilder cast = Builder.Cast(expression);

        // then
        Assert.NotNull(cast);
    }

    [Fact]
    public void CanBuildSumAggregateGivenVariableName()
    {
        // when
        var sum = Builder.Sum("s");

        // then
        Assert.Equal("SUM(?s)", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildSumAggregateGivenAnExpression()
    {
        // when
        var sum = Builder.Sum(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("SUM(STRLEN(?x))", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctSumAggregateGivenVariableName()
    {
        // when
        var sum = Builder.Distinct.Sum("s");

        // then
        Assert.Equal("SUM(DISTINCT ?s)", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildAvgAggregateGivenVariableName()
    {
        // when
        var sum = Builder.Avg("s");

        // then
        Assert.Equal("AVG(?s)", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildAvgAggregateGivenVariable()
    {
        // when
        var sum = Builder.Avg(new VariableTerm("s"));

        // then
        Assert.Equal("AVG(?s)", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildAvgAggregateGivenAnExpression()
    {
        // when
        var sum = Builder.Avg(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("AVG(STRLEN(?x))", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctAvgAggregateGivenVariableName()
    {
        // when
        var sum = Builder.Distinct.Avg("s");

        // then
        Assert.Equal("AVG(DISTINCT ?s)", sum.Expression.ToString());
    }

    [Fact]
    public void CanBuildMinAggregateGivenVariableName()
    {
        // when
        var min = Builder.Min("s");

        // then
        Assert.Equal("MIN(?s)", min.Expression.ToString());
    }

    [Fact]
    public void CanBuildMinAggregateGivenVariable()
    {
        // when
        var min = Builder.Min(new VariableTerm("s"));

        // then
        Assert.Equal("MIN(?s)", min.Expression.ToString());
    }

    [Fact]
    public void CanBuildMinAggregateGivenAnExpression()
    {
        // when
        var min = Builder.Min(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("MIN(STRLEN(?x))", min.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctMinAggregateGivenVariableName()
    {
        // when
        var min = Builder.Distinct.Min("s");

        // then
        Assert.Equal("MIN(DISTINCT ?s)", min.Expression.ToString());
    }

    [Fact]
    public void CanBuildMaxAggregateGivenVariableName()
    {
        // when
        var max = Builder.Max("s");

        // then
        Assert.Equal("MAX(?s)", max.Expression.ToString());
    }

    [Fact]
    public void CanBuildMaxAggregateGivenVariable()
    {
        // when
        var max = Builder.Max(new VariableTerm("s"));

        // then
        Assert.Equal("MAX(?s)", max.Expression.ToString());
    }

    [Fact]
    public void CanBuildMaxAggregateGivenAnExpression()
    {
        // when
        var max = Builder.Max(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("MAX(STRLEN(?x))", max.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctMaxAggregateGivenVariableName()
    {
        // when
        var max = Builder.Distinct.Max("s");

        // then
        Assert.Equal("MAX(DISTINCT ?s)", max.Expression.ToString());
    }

    [Fact]
    public void CanBuildSampleAggregateGivenVariableName()
    {
        // when
        var sample = Builder.Sample("s");

        // then
        Assert.Equal("SAMPLE(?s)", sample.Expression.ToString());
    }

    [Fact]
    public void CanBuildSampleAggregateGivenVariable()
    {
        // when
        var sample = Builder.Sample(new VariableTerm("s"));

        // then
        Assert.Equal("SAMPLE(?s)", sample.Expression.ToString());
    }

    [Fact]
    public void CanBuildSampleAggregateGivenAnExpression()
    {
        // when
        var sample = Builder.Sample(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("SAMPLE(STRLEN(?x))", sample.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenVariableName()
    {
        // when
        var groupConcat = Builder.GroupConcat("s");

        // then
        Assert.Equal("GROUP_CONCAT(?s)", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenVariable()
    {
        // when
        var groupConcat = Builder.GroupConcat(new VariableTerm("s"));

        // then
        Assert.Equal("GROUP_CONCAT(?s)", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenAnExpression()
    {
        // when
        var groupConcat = Builder.GroupConcat(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("GROUP_CONCAT(STRLEN(?x))", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctGroupConcatAggregateGivenVariableName()
    {
        // when
        var groupConcat = Builder.Distinct.GroupConcat("s");

        // then
        Assert.Equal("GROUP_CONCAT(DISTINCT ?s)", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenVariableNameWithSeparator()
    {
        // when
        var groupConcat = Builder.GroupConcat("s", ", ");

        // then
        Assert.Equal("GROUP_CONCAT(?s ; SEPARATOR = \", \")", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenVariableWithSeparator()
    {
        // when
        var groupConcat = Builder.GroupConcat(new VariableTerm("s"), ", ");

        // then
        Assert.Equal("GROUP_CONCAT(?s ; SEPARATOR = \", \")", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildGroupConcatAggregateGivenAnExpressionWithSeparator()
    {
        // when
        var groupConcat = Builder.GroupConcat(Builder.StrLen(Builder.Variable("x")), ", ");

        // then
        Assert.Equal("GROUP_CONCAT(STRLEN(?x) ; SEPARATOR = \", \")", groupConcat.Expression.ToString());
    }

    [Fact]
    public void CanBuildCountStarAggregateGivenVariableName()
    {
        // when
        var count = Builder.Count();

        // then
        Assert.Equal("COUNT(*)", count.Expression.ToString());
    }

    [Fact]
    public void CanBuildCountAggregateGivenVariableName()
    {
        // when
        var count = Builder.Count("s");

        // then
        Assert.Equal("COUNT(?s)", count.Expression.ToString());
    }

    [Fact]
    public void CanBuildCountAggregateGivenVariable()
    {
        // when
        var count = Builder.Count(new VariableTerm("s"));

        // then
        Assert.Equal("COUNT(?s)", count.Expression.ToString());
    }

    [Fact]
    public void CanBuildCountAggregateGivenAnExpression()
    {
        // when
        var count = Builder.Count(Builder.StrLen(Builder.Variable("x")));

        // then
        Assert.Equal("COUNT(STRLEN(?x))", count.Expression.ToString());
    }

    [Fact]
    public void CanBuildDisctinctCountAggregateGivenVariableName()
    {
        // when
        var count = Builder.Distinct.Count("s");

        // then
        Assert.Equal("COUNT(DISTINCT ?s)", count.Expression.ToString());
    }
}