namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class DynamicCollectionListTests
    {
        [Fact]
        public void Requires_node()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new DynamicCollectionList(null)
            );
        }

        [Fact]
        public void Requires_graph()
        {
            var r = new NodeFactory().CreateBlankNode();

            Assert.Throws<InvalidOperationException>(() =>
                new DynamicCollectionList(r)
            );
        }

        [Fact]
        public void Requires_list_root()
        {
            var g = new Graph();
            var r = g.CreateBlankNode();

            Assert.Throws<InvalidOperationException>(() =>
                new DynamicCollectionList(r)
            );
        }

        [Fact]
        public void Get_index_requires_positive_index()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p ([]) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var f = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var r = new DynamicCollectionList(f);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                r[-1]
            );
        }

        [Fact]
        public void Get_index_requires_existing_index()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p ([]) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                l[1]
            );
        }

        [Fact]
        public void Get_index_gets_index()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Equal("o", l[0]);
        }

        [Fact]
        public void Counts_list_items()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (0 1 2) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Equal(3, l.Count());
        }

        [Fact]
        public void Is_writable()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p ([]) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.False(l.IsReadOnly);
        }

        // TODO: rename
        [Fact]
        public void Add()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""o2"") .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            l.Add("o2");

            Assert.Equal(expected, g);
        }

        // TODO: Rename
        [Fact]
        public void Clear()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p [] .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""o2"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            l.Clear();

            Assert.Equal(expected, g);
        }

        // TODO: Rename
        [Fact]
        public void Contains()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Contains("o", l);
            Assert.DoesNotContain("o1", l);
        }

        // TODO: Remove to features
        [Fact]
        public void Handles_nested_lists1()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Contains("o1", l);
            Assert.Contains("o3", l);
            Assert.DoesNotContain("o2", l);
            Assert.IsAssignableFrom<IList<object>>(l[1]);
            Assert.Contains("o2", l[1] as IList<object>);
        }

        // TODO: Remove to features
        [Fact]
        public void Handles_nested_lists2()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var d = new DynamicGraph(null, new Uri("urn:"));
            d["s"] = new
            {
                p = new RdfCollection(
                    "o1" as object,
                    new RdfCollection(
                        "o2"
                    ),
                    "o3"
                )
            };

            Assert.Equal(expected as IGraph, d as IGraph);
        }
    }
}
