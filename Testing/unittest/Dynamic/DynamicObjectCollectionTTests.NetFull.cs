namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using VDS.RDF;
    using Xunit;

    public class DynamiObjectCollectionTTests
    {
        [Fact]
        public void Add_asserts_with_subject_predicate_and_argument_object()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:primitive> ""o"" .
");

            var g = new Graph();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var test = new Test(s);

            test.PrimitiveProperty.Add("o");

            Assert.Equal(expected, g);
        }

        [Fact]
        public void Contains_reports_by_subject_predicate_and_argument_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:primitive> ""s"" .
<urn:s> <urn:primitive> ""p"" .
<urn:s> <urn:primitive> ""o"" .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var test = new Test(s);

            Assert.Contains("s", test.PrimitiveProperty);
            Assert.Contains("p", test.PrimitiveProperty);
            Assert.Contains("o", test.PrimitiveProperty);
        }

        [Fact]
        public void Copies_objects_by_subject_and_predicate()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:primitive> ""s"" .
<urn:s> <urn:primitive> ""p"" .
<urn:s> <urn:primitive> ""o"" .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var test = new Test(s);

            var objects = new string[5]; // +2 for padding on each side
            test.PrimitiveProperty.CopyTo(objects, 1); // start at the second item at destination

            Assert.Equal(
                new[] { null, "s", "p", "o", null },
                objects);
        }

        [Fact]
        public void Enumerates_objects_by_subject_and_predicate()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:complex> <urn:s> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var test = new Test(s);

            var expected = new[] { s }.GetEnumerator();
            using (var actual = test.ComplexProperty.GetEnumerator())
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
        public void Remove_retracts_by_subject_predicate_and_argument_object()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:primitive> ""s"" .
<urn:s> <urn:primitive> ""p"" .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:primitive> ""s"" .
<urn:s> <urn:primitive> ""p"" .
<urn:s> <urn:primitive> ""o"" .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var test = new Test(s);

            test.PrimitiveProperty.Remove("o");

            Assert.Equal(
                expected,
                g);
        }

        internal class Test : DynamicNode
        {
            public Test(INode node) : base(node, new Uri("urn:")) { }

            public ICollection<Test> ComplexProperty => new DynamicObjectCollection<Test>(this, "complex");

            public ICollection<string> PrimitiveProperty => new DynamicObjectCollection<string>(this, "primitive");
        }
    }
}
