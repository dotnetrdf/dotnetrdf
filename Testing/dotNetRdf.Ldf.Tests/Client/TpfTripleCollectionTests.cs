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
using Xunit;

namespace VDS.RDF.LDF.Client;

[Collection("MockServer")]
public class TpfTripleCollectionTests(MockServer server)
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
        var constructor = () => new TpfTripleCollection(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the template was null");
    }

    [Fact(DisplayName = "Count enumerates when missing from metadata")]
    public void Count0() =>
        CollectionWithNoData.Count.Should().Be(0, "because metadata lacks triple count");

    [Fact(DisplayName = "Count is zero when negative")]
    public void CountNegative() =>
        CollectionFromMockData(MockServer.hasNegativeCount).Count.Should().Be(0, "because metadata is negative");

    [Fact(DisplayName = "Count is maximized to int")]
    public void CountMaximized() =>
        CollectionFromMockData(MockServer.hasLargeCount).Count.Should().Be(int.MaxValue, "because metadata was larger then an int");

    [Fact(DisplayName = "Count is populated from metadata")]
    public void CountFromMetadata()
    {
        var c = CollectionFromMockData(MockServer.hasCount, out var loader);
        var tripleCount = (int)loader.Metadata.TripleCount;

        c.Count.Should().Be(tripleCount, "because metadata has triple count");
    }

    [Fact(DisplayName = "Returns object nodes")]
    public void ObjectNodes() =>
        CollectionWithData.ObjectNodes.Should().Contain([o1, o2], "data has those objects");

    [Fact(DisplayName = "Returns predicate nodes")]
    public void PredicateNodes() =>
        CollectionWithData.PredicateNodes.Should().Contain([p1, p2], "because data has those predicates");

    [Fact(DisplayName = "Returns subject nodes")]
    public void SubjectNodes() =>
        CollectionWithData.SubjectNodes.Should().Contain([s1, s2], "because data has those subjects");

    [Fact(DisplayName = "Indexer throws for missing triple")]
    public void IndexerThrows()
    {
        var indexer = () => CollectionWithNoData[t1];

        indexer.Should().ThrowExactly<KeyNotFoundException>("because data lacks that triple");
    }

    [Fact(DisplayName = "Indexer returns contained triple")]
    public void IndexerReturnsContained() =>
        CollectionWithData[t1].Should().Be(t1, "because data has that triple");

    [Fact(DisplayName = "Contains is false for triple missing from data")]
    public void DoesNotContain() =>
        CollectionWithNoData.Contains(t1).Should().BeFalse("because data lacks that triple");

    [Fact(DisplayName = "Contains is true for triple in data")]
    public void DoesContains() =>
        CollectionWithData.Contains(t1).Should().BeTrue("because data has that triple");

    [Fact(DisplayName = "Dispose is a no-op")]
    public void Dispose()
    {
        var dispose = () => CollectionWithData.Dispose();

        dispose.Should().NotThrow("because it is disposable");
    }

    [Fact(DisplayName = "Enumerator is empty without data")]
    public void GetEnumeratorEmpty() =>
        CollectionWithNoData.Should().BeEmpty("because QPF data is empty");

    [Fact(DisplayName = "Enumerator contains data from QPF")]
    public void GetEnumeratorHasData() =>
        CollectionWithData.Should().Contain([t1, t2], "because QPF data has them");

    [Fact(DisplayName = "With object empty without data")]
    public void WithObjectEmpty() =>
        CollectionWithNoData.WithObject(o1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With object contains data from QPF")]
    public void WithObjectHasData() =>
        CollectionWithData.WithObject(o1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "With predicate empty without data")]
    public void WithPredicateEmpty() =>
        CollectionWithNoData.WithPredicate(p1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With predicate contains data from QPF")]
    public void WithPredicateHasData() =>
        CollectionWithData.WithPredicate(p1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "With predicate & object empty without data")]
    public void WithPredicateObjectEmpty() =>
        CollectionWithNoData.WithPredicateObject(p1, o1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With predicate & object contains data from QPF")]
    public void WithPredicateObjectHasData() =>
        CollectionWithData.WithPredicateObject(p1, o1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "With subject empty without data")]
    public void WithSubjectEmpty() =>
        CollectionWithNoData.WithSubject(s1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With subject contains data from QPF")]
    public void WithSubjectHasData() =>
        CollectionWithData.WithSubject(s1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "With subject & object empty without data")]
    public void WithSubjectObjectEmpty() =>
        CollectionWithNoData.WithSubjectObject(s1, o1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With subject & object contains data from QPF")]
    public void WithSubjectObjectHasData() =>
        CollectionWithData.WithSubjectObject(s1, o1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "With subject & predicate empty without data")]
    public void WithSubjectPredicateEmpty() =>
        CollectionWithNoData.WithSubjectPredicate(s1, p1).Should().BeEmpty("because it is missing from QPF data");

    [Fact(DisplayName = "With subject & predicate contains data from QPF")]
    public void WithSubjectPredicateHasData() =>
        CollectionWithData.WithSubjectPredicate(s1, p1).Should().Contain(t1, "because QPF data has that object");

    [Fact(DisplayName = "Asserted triples are the same as all triples")]
    public void AssertedSame()
    {
        var c = CollectionWithData;

        c.Asserted.Should().BeSameAs(c, "because QPF does not support RDF*");
    }

    #region Mutation & RDF*


    [Fact(DisplayName = "Cannot add triple")]
    public void CannotAdd()
    {
        var add = () => new Graph(CollectionWithNoData).Assert((Triple)null);

        add.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot remove triple")]
    public void CannotRemove()
    {
        var remove = () => new Graph(CollectionWithNoData).Retract((Triple)null);

        remove.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Contain no quoted triples")]
    public void NotContainsQuoted() =>
        CollectionWithNoData.ContainsQuoted(null).Should().BeFalse("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples are empty")]
    public void QuotedEmpty() =>
        CollectionWithNoData.Quoted.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triple count is 0")]
    public void QuotedCount0() =>
        CollectionWithNoData.QuotedCount.Should().Be(0, "because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted objects are empty")]
    public void QuotedObjectsEmpty() =>
        CollectionWithNoData.QuotedObjectNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted predicates are empty")]
    public void QuotedPredicatesEmpty() =>
        CollectionWithNoData.QuotedPredicateNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted subjects are empty")]
    public void QuotedSubjectsEmpty() =>
        CollectionWithNoData.QuotedSubjectNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with object are empty")]
    public void QuotedWithObjectEmpty() =>
        CollectionWithNoData.QuotedWithObject(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with predicate are empty")]
    public void QuotedWithPredicateEmpty() =>
        CollectionWithNoData.QuotedWithPredicate(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with predicate and object are empty")]
    public void QuotedWithPredicateObjectEmpty() =>
        CollectionWithNoData.QuotedWithPredicateObject(null, null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject are empty")]
    public void QuotedWithSubjectEmpty() =>
        CollectionWithNoData.QuotedWithSubject(null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject and object are empty")]
    public void QuotedWithSubjectObjectEmpty() =>
        CollectionWithNoData.QuotedWithSubjectObject(null, null).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject and predicate are empty")]
    public void QuotedWithSubjectPredicateEmpty() =>
        CollectionWithNoData.QuotedWithSubjectPredicate(null, null).Should().BeEmpty("because QPF does not support RDF*");

    #endregion

    private TpfTripleCollection CollectionWithNoData => CollectionFromMockData(MockServer.minimalControls);

    private TpfTripleCollection CollectionWithData => CollectionFromMockData(MockServer.multipleData);

    private TpfTripleCollection CollectionFromMockData(string name) => CollectionFromMockData(name, out var _);

    private TpfTripleCollection CollectionFromMockData(string name, out TpfLoader loader)
    {
        loader = new TpfLoader(new(server.BaseUri, name));
        var template = loader.Metadata.Search;
        return new TpfTripleCollection(template);
    }
}
