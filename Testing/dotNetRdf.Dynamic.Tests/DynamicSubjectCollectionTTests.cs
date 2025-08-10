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
using System.Collections.Generic;
using VDS.RDF;
using Xunit;

namespace VDS.RDF.Dynamic;

public class DynamicSubjectCollectionTTests
{
    [Fact]
    public void Add_asserts_with_predicate_object_and_argument_subject()
    {
        var expected = new Graph();
        expected.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

        var g = new Graph();
        var s = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:s")), g);
        var o = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:o")), g);

        o.P.Add(s);

        Assert.Equal(expected, g);
    }

    [Fact]
    public void Contains_reports_by_predicate_object_and_argument_subject()
    {
        var g = new Graph();
        g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:p> <urn:p> <urn:o> .
<urn:o> <urn:p> <urn:o> .
");

        var s = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:s")), g);
        var p = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:p")), g);
        var o = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:o")), g);

        Assert.True(o.P.Contains(s));
        Assert.True(o.P.Contains(p));
        Assert.True(o.P.Contains(o));
    }

    [Fact]
    public void Copies_subjects_by_predicate_and_object()
    {
        var g = new Graph();
        g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:p> <urn:p> <urn:o> .
<urn:o> <urn:p> <urn:o> .
");

        var s = g.CreateUriNode(UriFactory.Root.Create("urn:s"));
        var p = g.CreateUriNode(UriFactory.Root.Create("urn:p"));
        var o = g.CreateUriNode(UriFactory.Root.Create("urn:o"));
        var testO = new Test(o, g);

        var subjects = new Test[5]; // +2 for padding on each side
        testO.P.CopyTo(subjects, 1); // start at the second item at destination

        Assert.Equal(
            new[] { null, s, p, o, null },
            subjects);
    }

    [Fact]
    public void Enumerates_subjects_by_predicate_and_object()
    {
        var g = new Graph();
        g.LoadFromString(@"
<urn:s> <urn:p> <urn:s> .
");

        var s = g.CreateUriNode(UriFactory.Root.Create("urn:s"));
        var test = new Test(s, g);

        var expected = new[] { s }.GetEnumerator();
        using (var actual = test.P.GetEnumerator())
        {
            while (expected.MoveNext() | actual.MoveNext())
            {
                Assert.Equal(
                    expected.Current,
                    actual.Current);
            }
        }
    }

    [Fact]
    public void Remove_retracts_by_predicate_object_and_argument_subject()
    {
        var expected = new Graph();
        expected.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:p> <urn:p> <urn:o> .
");

        var g = new Graph();
        g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:p> <urn:p> <urn:o> .
<urn:o> <urn:p> <urn:o> .
");

        var o = new Test(g.CreateUriNode(UriFactory.Root.Create("urn:o")), g);

        o.P.Remove(o);

        Assert.Equal(
            expected,
            g);
    }

    internal class Test : DynamicNode
    {
        public Test(INode node, IGraph graph)
            : base(node, graph, new Uri("urn:"))
        {
        }

        public ICollection<Test> P => new DynamicSubjectCollection<Test>("p", this);
    }
}
