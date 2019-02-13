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
            public Test(INode node)
                : base(node, new Uri("urn:"))
            {
            }

            public ICollection<Test> ComplexProperty => new DynamicObjectCollection<Test>(this, "complex");

            public ICollection<string> PrimitiveProperty => new DynamicObjectCollection<string>(this, "primitive");
        }
    }
}
