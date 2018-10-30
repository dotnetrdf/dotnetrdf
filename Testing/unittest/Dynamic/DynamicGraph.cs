namespace VDS.RDF.Dynamic
{
    using System;
    using System.Dynamic;
    using System.Linq;
    using System.Linq.Expressions;
    using Xunit;

    public class DynamicGraphDynamicGraph
    {
        [Fact]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var d = new DynamicGraph();
            d.BaseUri = new Uri("urn:");

            Assert.Equal(d.BaseUri, d.SubjectBaseUri);
        }

        [Fact]
        public void Predicate_base_uri_defaults_to_subject_base_uri()
        {
            var d = new DynamicGraph(new Graph(), new Uri("urn:s"));

            Assert.Equal(d.SubjectBaseUri, d.PredicateBaseUri);
        }

        [Fact]
        public void Provides_meta_object()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");
            var p = (IDynamicMetaObjectProvider)d;
            var mo = p.GetMetaObject(Expression.Empty());
            var n = mo.GetDynamicMemberNames();

            Assert.Single(n);
            Assert.Equal("urn:s", n.Single());
        }
    }
}
