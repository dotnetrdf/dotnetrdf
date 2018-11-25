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
                new DynamicNode(s)
            );
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
                ((IUriNode)d).Uri
            );
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
                ((IBlankNode)d).InternalID
            );
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
