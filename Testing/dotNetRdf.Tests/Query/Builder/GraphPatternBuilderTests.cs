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
using Xunit;
using Moq;
using VDS.RDF.Query.Expressions;
using VDS.RDF.Query.Expressions.Functions.Sparql.Boolean;
using VDS.RDF.Query.Expressions.Primary;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder;


public class GraphPatternBuilderTests
{
    private GraphPatternBuilder _builder;
    private Mock<INamespaceMapper> _namespaceMapper;

    public GraphPatternBuilderTests()
    {
        _namespaceMapper = new Mock<INamespaceMapper>(MockBehavior.Strict);
        _builder = new GraphPatternBuilder();
    }

    [Fact]
    public void ShouldAllowUsingISparqlExpressionForFilter()
    {
        // given
        ISparqlExpression expression = new IsIriFunction(new VariableTerm("x"));
        _builder.Filter(expression);

        // when
        GraphPattern graphPattern = _builder.BuildGraphPattern(_namespaceMapper.Object);

        // then
        Assert.True(graphPattern.IsFiltered);
        Assert.Same(expression, graphPattern.Filter.Expression);
    }

    [Fact]
    public void ShouldAllowAddingSimpleChildGraphPatterns()
    {
        // given
        _builder.Child(cp => cp.Child(cp2 => cp2.Child(cp3 => cp3.Child(cp4 => cp4.Child(last => { })))));

        // when
        var graphPattern = _builder.BuildGraphPattern(_namespaceMapper.Object);

        // then
        for (var i = 0; i < 4; i++)
        {
            Assert.Single(graphPattern.ChildGraphPatterns);
            graphPattern = graphPattern.ChildGraphPatterns.Single();
        }
    }
}