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

            Assert.Equal(expected as IGraph, g as IGraph);
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

            Assert.Equal(new Uri("urn:") as object, d.BaseUri as object);
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
