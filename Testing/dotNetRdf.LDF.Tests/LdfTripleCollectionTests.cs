/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using FluentAssertions;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace VDS.RDF.LDF;

[Collection("MockServer")]
public class LdfTripleCollectionTests(MockServer server)
{
    private readonly static NodeFactory factory = new();
    private readonly static INode s1 = factory.CreateUriNode(UriFactory.Create("urn:example:s1"));
    private readonly static INode p1 = factory.CreateUriNode(UriFactory.Create("urn:example:p1"));
    private readonly static INode o1 = factory.CreateUriNode(UriFactory.Create("urn:example:o1"));
    private readonly static INode s2 = factory.CreateUriNode(UriFactory.Create("urn:example:s2"));
    private readonly static INode p2 = factory.CreateUriNode(UriFactory.Create("urn:example:p2"));
    private readonly static INode o2 = factory.CreateUriNode(UriFactory.Create("urn:example:o2"));
    private readonly static Triple t1 = new(s1, p1, o1);
    private readonly static Triple t2 = new(s2, p2, o2);

    [Fact(DisplayName = "Requires template")]
    public void RequiresTemplate()
    {
        var constructor = () => new LdfTripleCollection(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the template was null");
    }

    [Fact(DisplayName = "Count is zero when missing from metadata")]
    public void Count0()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Count.Should().Be(0, "because metadata lacks triple count");
    }

    [Fact(DisplayName = "Count is zero when negative")]
    public void CountNegative()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Count.Should().Be(0, "because metadata is negative");
    }

    [Fact(DisplayName = "Count is maximized to int")]
    public void CountMaximized()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.hasLargeCount));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Count.Should().Be(int.MaxValue, "because metadata was larger then an int");
    }

    [Fact(DisplayName = "Count is populated from metadata")]
    public void CountFromMetadata()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.hasCount));
        var template = loader.Metadata.Search;
        var tripleCount = (int)loader.Metadata.TripleCount;
        var c = new LdfTripleCollection(template);

        c.Count.Should().Be(tripleCount, "because metadata has triple count");
    }

    [Fact(DisplayName = "Returns object nodes")]
    public void ObjectNodes()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.ObjectNodes.Should().Contain([o1, o2], "data has those objects");
    }

    [Fact(DisplayName = "Returns predicate nodes")]
    public void PredicateNodes()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.PredicateNodes.Should().Contain([p1, p2], "because data has those predicates");
    }

    [Fact(DisplayName = "Returns subject nodes")]
    public void SubjectNodes()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.SubjectNodes.Should().Contain([s1, s2], "because data has those subjects");
    }

    [Fact(DisplayName = "Indexer throws for missing triple")]
    public void IndexerThrows()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        var indexer = () => c[t1];

        indexer.Should().ThrowExactly<KeyNotFoundException>("because data lacks that triple");
    }

    [Fact(DisplayName = "Indexer returns contained triple")]
    public void IndexerReturnsContained()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c[t1].Should().Be(t1, "because data has that triple");
    }

    [Fact(DisplayName = "Contains is false for triple missing from data")]
    public void DoesNotContain()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Contains(t1).Should().BeFalse("because data lacks that triple");
    }

    [Fact(DisplayName = "Contains is true for triple in data")]
    public void DoesContains()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Contains(t1).Should().BeTrue("because data has that triple");
    }

    [Fact(DisplayName = "Dispose is a no-op")]
    public void Dispose()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        var dispose = () => c.Dispose();

        dispose.Should().NotThrow("because it is disposable");
    }

    [Fact(DisplayName = "Enumerator is empty without data")]
    public void GetEnumeratorEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Should().BeEmpty("because QPF data is empty");
    }

    [Fact(DisplayName = "Enumerator contains data from QPF")]
    public void GetEnumeratorHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Should().Contain([t1, t2], "because QPF data has them");
    }

    [Fact(DisplayName = "With object empty without data")]
    public void WithObjectEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithObject(o1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With object contains data from QPF")]
    public void WithObjectHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithObject(o1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "With predicate empty without data")]
    public void WithPredicateEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithPredicate(p1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With predicate contains data from QPF")]
    public void WithPredicateHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithPredicate(p1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "With predicate & object empty without data")]
    public void WithPredicateObjectEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithPredicateObject(p1, o1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With predicate & object contains data from QPF")]
    public void WithPredicateObjectHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithPredicateObject(p1, o1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "With subject empty without data")]
    public void WithSubjectEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubject(s1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With subject contains data from QPF")]
    public void WithSubjectHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubject(s1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "With subject & object empty without data")]
    public void WithSubjectObjectEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubjectObject(s1, o1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With subject & object contains data from QPF")]
    public void WithSubjectObjectHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubjectObject(s1, o1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "With subject & predicate empty without data")]
    public void WithSubjectPredicateEmpty()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.minimalControls));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubjectPredicate(s1, p1).Should().BeEmpty("because it is missing from QPF data");
    }

    [Fact(DisplayName = "With subject & predicate contains data from QPF")]
    public void WithSubjectPredicateHasData()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.WithSubjectPredicate(s1, p1).Should().Contain(t1, "because QPF data has that object");
    }

    [Fact(DisplayName = "Asserted triples are the same as all triples")]
    public void AssertedSame()
    {
        using var loader = new LdfLoader(new(server.BaseUri, MockServer.multipleData));
        var template = loader.Metadata.Search;
        var c = new LdfTripleCollection(template);

        c.Asserted.Should().BeSameAs(c, "because QPF does not support RDF*");
    }




    #region Mutation & RDF*


    [Fact(DisplayName = "Cannot add triple")]
    public void CannotAdd()
    {
        var add = () => new Graph(AnyLdfTriplesCollection()).Assert((Triple)null);

        add.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot remove triple")]
    public void CannotRemove()
    {
        var remove = () => new Graph(AnyLdfTriplesCollection()).Retract((Triple)null);

        remove.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Contain no quoted triples")]
    public void NotContainsQuoted() => AnyLdfTriplesCollection().ContainsQuoted(null).Should().BeFalse("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples are empty")]
    public void QuotedEmpty() => AnyLdfTriplesCollection().Quoted.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triple count is 0")]
    public void QuotedCount0() => AnyLdfTriplesCollection().QuotedCount.Should().Be(0, "because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted objects are empty")]
    public void QuotedObjectsEmpty() => AnyLdfTriplesCollection().QuotedObjectNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted predicates are empty")]
    public void QuotedPredicatesEmpty() => AnyLdfTriplesCollection().QuotedPredicateNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted subjects are empty")]
    public void QuotedSubjectsEmpty() => AnyLdfTriplesCollection().QuotedSubjectNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with object are empty")]
    public void QuotedWithObjectEmpty() => AnyLdfTriplesCollection().QuotedWithObject(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with predicate are empty")]
    public void QuotedWithPredicateEmpty() => AnyLdfTriplesCollection().QuotedWithPredicate(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with predicate and object are empty")]
    public void QuotedWithPredicateObjectEmpty() => AnyLdfTriplesCollection().QuotedWithPredicateObject(null, null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject are empty")]
    public void QuotedWithSubjectEmpty() => AnyLdfTriplesCollection().QuotedWithSubject(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject and object are empty")]
    public void QuotedWithSubjectObjectEmpty() => AnyLdfTriplesCollection().QuotedWithSubjectObject(null, null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject and predicate are empty")]
    public void QuotedWithSubjectPredicateEmpty() => AnyLdfTriplesCollection().QuotedWithSubjectPredicate(null, null).Should().BeEmpty("because QPF does not support RDF*");

    private LdfTripleCollection AnyLdfTriplesCollection() => new(new LdfLoader(new(server.BaseUri, MockServer.minimalControls)).Metadata.Search);

    #endregion
}
