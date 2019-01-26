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
    using System.Dynamic;
    using System.Linq.Expressions;
    using VDS.RDF;
    using Xunit;

    public class DictionaryMetaObjectTests
    {
        [Fact]
        public void Handles_get_member()
        {
            var g = new DynamicGraph(subjectBaseUri: new Uri("urn:"));
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            dynamic d = g;

            Assert.Equal(s, d.s);
        }

        [Fact]
        public void Handles_set_member()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
");

            var g = new DynamicGraph(subjectBaseUri: new Uri("urn:"));
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            dynamic d = g;

            d.s = new { p = "o" };

            Assert.Equal<IGraph>(expected, g);
        }

        [Fact]
        public void Handles_member_names()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var provider = g as IDynamicMetaObjectProvider;
            var meta = provider.GetMetaObject(Expression.Parameter(typeof(object), "debug"));

            var names = meta.GetDynamicMemberNames();

            Assert.Equal(new[] { "urn:s", "urn:o" }, names);
        }

        [Fact]
        public void Existing_get_members_pass_through()
        {
            var g = new DynamicGraph { BaseUri = new Uri("urn:") };
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            dynamic d = g;

            Assert.Equal<object>(new Uri("urn:"), d.BaseUri);
        }

        [Fact]
        public void Existing_set_members_pass_through()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            dynamic d = g;

            d.BaseUri = new Uri("urn:");

            Assert.Equal(new Uri("urn:"), g.BaseUri);
        }

        [Fact]
        public void Existing_methods_pass_through()
        {
            var g = new DynamicGraph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            dynamic d = g;

            d.Clear();

            Assert.True(g.IsEmpty);
        }
    }
}
