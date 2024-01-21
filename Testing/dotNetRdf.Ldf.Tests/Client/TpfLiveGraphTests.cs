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
using FluentAssertions.Execution;
using System;
using System.Collections.Generic;
using Xunit;

namespace VDS.RDF.LDF.Client;

[Collection("MockServer")]
public class TpfLiveGraphTests(MockServer server)
{
    [Fact(DisplayName = "Requires base URI")]
    public void RequiresBaseUri()
    {
        var constructor = () => new TpfLiveGraph(null);

        constructor.Should().ThrowExactly<ArgumentNullException>("because the base URI was null");
    }

    [Fact(DisplayName = "Supports underlying brute-force equality checking (positive)")]
    public void UnderlyingEquality()
    {
        var other = new Graph();
        other.Assert(
            other.CreateUriNode(other.UriFactory.Create("urn:example:s1")),
            other.CreateUriNode(other.UriFactory.Create("urn:example:p1")),
            other.CreateUriNode(other.UriFactory.Create("urn:example:o1")));

        using (new AssertionScope())
        {
            GraphWithData.Equals(other, out var mapping).Should().BeTrue("because triples are the same");
            mapping.Should().BeNull("because LDF does not support blank nodes");
        }
    }

    [Fact(DisplayName = "Supports underlying brute-force equality checking (negative)")]
    public void UnderlyingEquality2()
    {
        var other = new Graph();
        other.Assert(
            other.CreateBlankNode(),
            other.CreateUriNode(other.UriFactory.Create("urn:example:p1")),
            other.CreateUriNode(other.UriFactory.Create("urn:example:o1")));

        using (new AssertionScope())
        {
            GraphWithData.Equals(other, out var mapping).Should().BeFalse("because triples differ");
            mapping.Should().BeNull("because LDF does not support blank nodes");
        }
    }

    [Fact(DisplayName = "Two LDF graphs with the same search templates are equal")]
    public void TemplateEquality()
    {
        using (new AssertionScope())
        {
            GraphWithNoData.Equals(GraphWithNoData, out var mapping).Should().BeTrue("because search templates are equal");
            mapping.Should().BeNull("because LDF does not support blank nodes");
        }
    }

    [Fact(DisplayName = "Two LDF graphs with different search templates are not equal")]
    public void TemplateInequality()
    {
        using (new AssertionScope())
        {
            GraphWithNoData.Equals(GraphWithData, out var mapping).Should().BeFalse("because search templates differ");
            mapping.Should().BeNull("because LDF does not support blank nodes");
        }
    }

    #region Mutation & RDF*

    [Fact(DisplayName = "Cannot assert")]
    public void CannotAssert()
    {
        var assert = () => GraphWithNoData.Assert(null as Triple);

        assert.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot assert multiple")]
    public void CannotAssertMultiple()
    {
        var assert = () => GraphWithNoData.Assert(null as IEnumerable<Triple>);

        assert.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot clear")]
    public void CannotClear()
    {
        var clear = () => GraphWithNoData.Clear();

        clear.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot merge")]
    public void CannotMerge()
    {
        var merge = () => GraphWithNoData.Merge(default);

        merge.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot merge retaining graph URI")]
    public void CannotMergeVerbose()
    {
        var merge = () => GraphWithNoData.Merge(default, default);

        merge.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot retract")]
    public void CannotRetract()
    {
        var retract = () => GraphWithNoData.Retract(null as Triple);

        retract.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "Cannot retract multiple")]
    public void CannotRetractMultiple()
    {
        var retract = () => GraphWithNoData.Retract(null as IEnumerable<Triple>);

        retract.Should().ThrowExactly<NotSupportedException>("because QPF does not support mutation");
    }

    [Fact(DisplayName = "All quoted nodes are empty")]
    public void AllQuotedNodesEmpty() =>
        GraphWithNoData.AllQuotedNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Does not contain quoted triple")]
    public void NotContainQuoted() =>
        GraphWithNoData.ContainsQuotedTriple(default).Should().BeFalse("because QPF does not support RDF*");

    [Fact(DisplayName = "Gets null blank nodes")]
    public void BlankNull() =>
        GraphWithNoData.GetBlankNode(default).Should().BeNull("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with node are empty")]
    public void QuotedEmptyNode() =>
        GraphWithNoData.GetQuoted(null as INode).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with URI are empty")]
    public void QuotedEmptyUri() =>
        GraphWithNoData.GetQuoted(null as Uri).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with URI object are empty")]
    public void QuotedWithObjectEmptyUri() =>
        GraphWithNoData.GetQuotedWithObject(null as Uri).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with node object are empty")]
    public void QuotedWithObjectEmptyNode() =>
        GraphWithNoData.GetQuotedWithObject(null as INode).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with URI predicate are empty")]
    public void QuotedWithPredicateEmptyUri() =>
        GraphWithNoData.GetQuotedWithPredicate(null as Uri).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with node predicate are empty")]
    public void QuotedWithPredicateEmptyNode() =>
        GraphWithNoData.GetQuotedWithPredicate(null as INode).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with URI subject are empty")]
    public void QuotedWithSubjectEmptyUri() =>
        GraphWithNoData.GetQuotedWithSubject(null as Uri).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with node subject are empty")]
    public void QuotedWithSubjectEmptyNode() =>
        GraphWithNoData.GetQuotedWithSubject(null as INode).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject & predicate are empty")]
    public void QuotedWithSubjectPredicateEmpty() =>
        GraphWithNoData.GetQuotedWithSubjectPredicate(default, default).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with subject & object are empty")]
    public void QuotedWithSubjectObjectEmpty() =>
        GraphWithNoData.GetQuotedWithSubjectObject(default, default).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples with predicate & object are empty")]
    public void QuotedWithPredicateObjectEmpty() =>
        GraphWithNoData.GetQuotedWithPredicateObject(default, default).Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted nodes are empty")]
    public void QuotedNodesEmpty() =>
        GraphWithNoData.QuotedNodes.Should().BeEmpty("because QPF does not support RDF*");

    [Fact(DisplayName = "Quoted triples are empty")]
    public void QuotedTriplesEmpty() =>
        GraphWithNoData.QuotedTriples.Should().BeEmpty("because QPF does not support RDF*");

    #endregion

    private TpfLiveGraph GraphWithNoData => GrpahFromMockData(MockServer.minimalControls);

    private TpfLiveGraph GraphWithData => GrpahFromMockData(MockServer.singleData);

    private TpfLiveGraph GrpahFromMockData(string name)
    {
        return new TpfLiveGraph(new(server.BaseUri, name));
    }

}
