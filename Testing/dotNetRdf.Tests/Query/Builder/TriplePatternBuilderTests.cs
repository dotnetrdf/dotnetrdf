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
using VDS.RDF.Parsing;
using VDS.RDF.Query.Patterns;

namespace VDS.RDF.Query.Builder;


public class TriplePatternBuilderTests : IDisposable
{
    private TriplePatternBuilder _builder;
    private Mock<INamespaceMapper> _namespaceMapper;

    public TriplePatternBuilderTests()
    {
        _namespaceMapper = new Mock<INamespaceMapper>(MockBehavior.Strict);
        _builder = new TriplePatternBuilder(_namespaceMapper.Object);
    }

    public void Dispose()
    {
        _namespaceMapper.VerifyAll();
    }

    [Fact]
    public void CanCreateTriplePatternUsingVariableNames()
    {
        // when
        _builder.Subject("s").Predicate("p").Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern) _builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new [] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingUriForPredicate()
    {
        // given
        var predicateUri = new Uri("http://www.example.com/property");

        // when
        _builder.Subject("s").PredicateUri(predicateUri).Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is NodeMatchPattern);
        Assert.Equal(new Uri("http://www.example.com/property"), ((dynamic)pattern.Predicate).Node.Uri);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingQNameForSubject()
    {
        // given
        const string predicateQName = "foaf:name";
        _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        _builder.Subject<IUriNode>(predicateQName).Predicate("p").Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is NodeMatchPattern);
        Assert.Equal(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Subject).Node.Uri);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingQNameForPredicate()
    {
        // given
        const string predicateQName = "foaf:name";
        _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        _builder.Subject("s").PredicateUri(predicateQName).Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is NodeMatchPattern);
        Assert.Equal(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Predicate).Node.Uri);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingUriForSubject()
    {
        // when
        _builder.Subject(new Uri("http://xmlns.com/foaf/0.1/name")).Predicate("p").Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is NodeMatchPattern);
        Assert.Equal(new Uri("http://xmlns.com/foaf/0.1/name"), ((dynamic)pattern.Subject).Node.Uri);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingUriForObject()
    {
        // when
        _builder.Subject("s").Predicate("p").Object(new Uri("http://xmlns.com/foaf/0.1/Person"));

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal(new Uri("http://xmlns.com/foaf/0.1/Person"), ((dynamic)pattern.Object).Node.Uri);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingQNameForObject()
    {
        // given
        const string predicateQName = "foaf:Person";
        _namespaceMapper.Setup(m => m.GetNamespaceUri("foaf")).Returns(new Uri("http://xmlns.com/foaf/0.1/"));

        // when
        _builder.Subject("s").Predicate("p").Object<IUriNode>(predicateQName);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal(new Uri("http://xmlns.com/foaf/0.1/Person"), ((dynamic)pattern.Object).Node.Uri);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingBlankNodeForObject()
    {
        // when
        _builder.Subject("s").Predicate("p").Object<IBlankNode>("bnode");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is BlankNodePattern);
        Assert.Equal("_:bnode", ((BlankNodePattern)pattern.Object).ID);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingBlankNodeForSubject()
    {
        // when
        _builder.Subject<IBlankNode>("s").Predicate("p").Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is BlankNodePattern);
        Assert.Equal("_:s", ((BlankNodePattern)pattern.Subject).ID);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingINodeForSubject()
    {
        // given
        var node = new NodeFactory().CreateBlankNode("bnode");

        // when
        _builder.Subject(node).Predicate("p").Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is NodeMatchPattern);
        Assert.Same(node, ((NodeMatchPattern)pattern.Subject).Node);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingIUriNodeForPredicate()
    {
        // given
        var node = new NodeFactory().CreateUriNode(new Uri("http://www.example.com/predicate"));

        // when
        _builder.Subject("s").PredicateUri(node).Object("o");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
        Assert.True(pattern.Predicate is NodeMatchPattern);
        Assert.Same(node, ((NodeMatchPattern)pattern.Predicate).Node);
        Assert.True(pattern.Object is VariablePattern);
        Assert.Equal(new[] { "o" }, pattern.Object.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingINodeForObject()
    {
        // given
        var node = new NodeFactory().CreateUriNode(new Uri("http://www.example.com/object"));

        // when
        _builder.Subject("s").Predicate("p").Object(node);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Same(node, ((NodeMatchPattern)pattern.Object).Node);
        Assert.True(pattern.Predicate is VariablePattern);
        Assert.Equal(new[] { "p" }, pattern.Predicate.Variables);
        Assert.True(pattern.Subject is VariablePattern);
        Assert.Equal(new[] { "s" }, pattern.Subject.Variables);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingIntegerLiteralObject()
    {
        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(42);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal("42", ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, XmlSpecsHelper.XmlSchemaDataTypeString);
        Assert.True(string.IsNullOrWhiteSpace(((dynamic)pattern.Object).Node.Language));
    }

    [Fact]
    public void CanCreateTriplePatternsUsingTypedLiteralObject()
    {
        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(42, new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger));

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal("42", ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(new Uri(XmlSpecsHelper.XmlSchemaDataTypeInteger), ((dynamic)pattern.Object).Node.DataType);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingLiteralObjectWithLanguageTag()
    {
        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(42, "pl-PL");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal("42", ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, RdfSpecsHelper.RdfLangString);
        Assert.Equal("pl-pl", ((dynamic)pattern.Object).Node.Language);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingLiteralObjectWithLanguageTag2()
    {
        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(42, "pl-PL");

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal("42", ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, RdfSpecsHelper.RdfLangString);
        Assert.Equal("pl-pl", ((dynamic)pattern.Object).Node.Language);
    }

    [Fact]
    public void CanCreateTriplePatternsUsingDateLiteralObject()
    {
        // given
        var dateTime = new DateTime(2012, 10, 13);

        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal(dateTime.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, XmlSpecsHelper.XmlSchemaDataTypeString);
        Assert.True(string.IsNullOrWhiteSpace(((dynamic)pattern.Object).Node.Language));
    }

    [Fact]
    public void CanCreateTriplePatternsUsingDateTimeLiteralObject()
    {
        // given
        var dateTime = new DateTime(2012, 10, 13, 15, 45, 15);

        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal(dateTime.ToString(XmlSpecsHelper.XmlSchemaDateTimeFormat), ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, XmlSpecsHelper.XmlSchemaDataTypeString);
        Assert.True(string.IsNullOrWhiteSpace(((dynamic)pattern.Object).Node.Language));
    }

    [Fact]
    public void CanCreateTriplePatternsUsingDateTimeOffsetLiteralObject()
    {
        // given
        var dateTime = new DateTimeOffset(2012, 10, 13, 20, 35, 10, new TimeSpan(0, 1, 30, 0));

        // when
        _builder.Subject("s").Predicate("p").ObjectLiteral(dateTime);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.True(pattern.Object is NodeMatchPattern);
        Assert.Equal("2012-10-13T20:35:10.000000+01:30", ((dynamic)pattern.Object).Node.Value);
        Assert.Equal(((dynamic)pattern.Object).Node.DataType.AbsoluteUri, XmlSpecsHelper.XmlSchemaDataTypeString);
        Assert.True(string.IsNullOrWhiteSpace(((dynamic)pattern.Object).Node.Language));
    }

    [Fact]
    public void CanCreateTriplePatternsUsingActualPatternItems()
    {
        // given
        PatternItem s = new VariablePattern("s");
        PatternItem p = new VariablePattern("p");
        PatternItem o = new VariablePattern("o");

        // when
        _builder.Subject(s).Predicate(p).Object(o);

        // then
        Assert.Single(_builder.Patterns);
        var pattern = (IMatchTriplePattern)_builder.Patterns.Single();
        Assert.Same(s, pattern.Subject);
        Assert.Same(p, pattern.Predicate);
        Assert.Same(o, pattern.Object);
    }
}