namespace VDS.RDF.Dynamic
{
    using System.Dynamic;
    using System.Linq.Expressions;
    using Xunit;

    public class DynamicGraphTests
    {
        [Fact]
        public void Subject_base_uri_defaults_to_graph_base_uri()
        {
            var d = new DynamicGraph();
            d.BaseUri = UriFactory.Create("urn:");

            Assert.Equal(d.BaseUri, d.SubjectBaseUri);
        }

        [Fact]
        public void Predicate_base_uri_defaults_to_subject_base_uri()
        {
            var d = new DynamicGraph(new Graph(), UriFactory.Create("urn:s"));

            Assert.Equal(d.SubjectBaseUri, d.PredicateBaseUri);
        }

        [Fact]
        public void Provides_dictionary_meta_object()
        {
            var d = new DynamicGraph();
            d.LoadFromString(@"
<urn:s> <urn:p> <urn:o> .
");

            var p = (IDynamicMetaObjectProvider)d;
            var mo = p.GetMetaObject(Expression.Empty());

            Assert.NotNull(mo);
        }
    }
}
