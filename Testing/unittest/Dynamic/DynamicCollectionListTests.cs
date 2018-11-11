namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
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

        // TODO: Rename
        [Fact]
        public void Get_index()
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
        public void Set_index_requires_positive_index()
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
                r[-1] = null
            );
        }

        [Fact]
        public void Set_index_requires_existing_index()
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
                l[1] = null
            );
        }

        [Fact]
        public void Set_index_requires_value()
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

            Assert.Throws<ArgumentNullException>(() =>
                l[0] = null
            );
        }

        // TODO: Rename
        [Fact]
        public void Set_index()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""oX"" ""o3"") .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""o2"" ""o3"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            l[1] = "oX";

            Assert.Equal(expected, g);
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

        // TODO: Rename
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

        // TODO: Rename
        [Fact]
        public void CopyTo()
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

            var objects = new object[1];
            l.CopyTo(objects, 0);

            Assert.Equal(new[] { "o" }, objects);
        }

        // TODO: Rename
        [Fact]
        public void GetEnumerator()
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

            using (var enumerator = l.GetEnumerator())
            {
                enumerator.MoveNext();

                Assert.Equal("o", enumerator.Current);
            }
        }

        // TODO: Rename
        [Fact]
        public void IndexOf()
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

            Assert.Equal(0, l.IndexOf("o"));
            Assert.Equal(-1, l.IndexOf("o1"));
        }

        [Fact]
        public void Insert_requires_positive_index()
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
                r.Insert(-1, null)
            );
        }

        [Fact]
        public void Insert_requires_existing_index()
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
                r.Insert(1, null)
            );
        }

        [Fact]
        public void Insert_requires_value()
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

            Assert.Throws<ArgumentNullException>(() =>
                r.Insert(0, null)
            );
        }

        // TODO: Rename
        [Fact]
        public void Insert()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o"" []) .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p ([]) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var f = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var r = new DynamicCollectionList(f);

            r.Insert(0, "o");

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Remove_requires_value()
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

            Assert.Throws<ArgumentNullException>(() =>
                r.Remove(null)
            );
        }

        [Fact]
        public void Remove_ignores_missing_value()
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

            Assert.False(r.Remove("o"));
        }

        // TODO: Rename
        [Fact]
        public void Remove()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p [] .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var f = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var r = new DynamicCollectionList(f);

            var result = r.Remove("o");

            Assert.True(result);
            Assert.Equal(expected, g);
        }

        [Fact]
        public void Remove_at_requires_positive_index()
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
                r.RemoveAt(-1)
            );
        }

        [Fact]
        public void Remove_at_requires_existing_index()
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
                r.RemoveAt(1)
            );
        }

        // TODO: Rename
        [Fact]
        public void Remove_at()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""o3"") .
");

            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" ""o2"" ""o3"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var f = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var r = new DynamicCollectionList(f);

            r.RemoveAt(1);

            Assert.Equal(expected, g);
        }

        // TODO: Rename
        [Fact]
        public void EnumerableGetEnumerator()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r) as IEnumerable;

            var enumerator = l.GetEnumerator();
            enumerator.MoveNext();

            Assert.Equal("o", enumerator.Current);
        }

        [Fact]
        public void Provides_meta_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (:o) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r) as IDynamicMetaObjectProvider;
            var mo = l.GetMetaObject(Expression.Empty());

            Assert.IsType<EnumerableMetaObject>(mo);
        }
    }
}
