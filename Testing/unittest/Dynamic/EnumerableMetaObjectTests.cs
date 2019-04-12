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
    using System.Linq;
    using VDS.RDF;
    using Xunit;

    public class EnumerableMetaObjectTests
    {
        [Fact]
        public void Fails_no_generic_type_arguments()
        {
            var g = new Graph();
            var s = g.CreateBlankNode();
            var p = g.CreateBlankNode();
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            Assert.Throws<InvalidOperationException>(() =>
                objects.Average());
        }

        [Fact]
        public void Handles_one_generic_type_argument()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var o = g.CreateUriNode(UriFactory.Create("urn:o"));
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            Assert.Equal(o, objects.Single());
        }

        [Fact]
        public void Handles_two_generic_type_arguments()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            Func<object, object> selector = n => n.ToString();

            Assert.Equal(new[] { "urn:o" }, objects.Select(selector));
        }

        [Fact]
        public void Handles_three_generic_type_arguments()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> ""a""@en .
<urn:s> <urn:p> ""b""@en .
<urn:s> <urn:p> ""c""@fr .
<urn:s> <urn:p> ""d""@fr .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            Func<object, string> keySelector = n => ((ILiteralNode)n).Language;
            Func<object, string> elementSelector = n => ((ILiteralNode)n).Value;

            var result = objects.GroupBy(keySelector, elementSelector);

            Assert.Collection(
                (IEnumerable<IGrouping<object, object>>)result,
                group =>
                {
                    Assert.Equal("en", group.Key);
                    Assert.Equal(new[] { "a", "b" }, group);
                },
                group =>
                {
                    Assert.Equal("fr", group.Key);
                    Assert.Equal(new[] { "c", "d" }, group);
                });
        }

        [Fact]
        public void Existing_methods_pass_through()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:s> <urn:o> .
");

            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
<urn:s> <urn:s> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            objects.Clear();

            Assert.Equal(expected, g);
        }
    }
}
