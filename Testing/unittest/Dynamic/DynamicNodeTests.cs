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
    using Xunit;

    public class DynamicNodeTests
    {
        [Fact]
        public void Requires_node_with_graph()
        {
            var s = new NodeFactory().CreateBlankNode();

            Assert.Throws<InvalidOperationException>(() =>
                new DynamicNode(s));
        }

        [Fact]
        public void BaseUri_defaults_to_graph_base_uri()
        {
            var g = new Graph { BaseUri = new Uri("urn:g") };
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = new DynamicNode(s);

            Assert.Equal(g.BaseUri, d.BaseUri);
        }

        [Fact]
        public void Can_act_like_uri_node()
        {
            var g = new Graph();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = new DynamicNode(s);

            Assert.IsAssignableFrom<IUriNode>(d);
            Assert.Equal(((IUriNode)d).Uri, s.Uri);
        }

        [Fact]
        public void Uri_fails_if_underlying_node_is_not_uri()
        {
            var g = new Graph();
            var s = g.CreateBlankNode();
            var d = new DynamicNode(s);

            Assert.Throws<InvalidOperationException>(() =>
                ((IUriNode)d).Uri);
        }

        [Fact]
        public void Can_act_like_blank_node()
        {
            var g = new Graph();
            var s = g.CreateBlankNode();
            var d = new DynamicNode(s);

            Assert.IsAssignableFrom<IBlankNode>(d);
            Assert.Equal(((IBlankNode)d).InternalID, s.InternalID);
        }

        [Fact]
        public void InternalId_fails_if_underlying_node_is_not_blank()
        {
            var g = new Graph();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = new DynamicNode(s);

            Assert.Throws<InvalidOperationException>(() =>
                ((IBlankNode)d).InternalID);
        }

        [Fact]
        public void Provides_dictionary_meta_object()
        {
            var g = new Graph();
            g.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var d = new DynamicNode(s);
            var p = (IDynamicMetaObjectProvider)d;
            var mo = p.GetMetaObject(Expression.Empty());

            Assert.NotNull(mo);
        }
    }
}
