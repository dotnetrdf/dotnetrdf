namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Dynamic;
    using Xunit;

    public class EnumerableMetaObjectTests
    {
        [Fact]
        public void No_generic_type_arguments()
        {
            var g = new DynamicGraph();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));
            var p = g.CreateUriNode(UriFactory.Create("urn:p"));
            var d = new DynamicNode(s);
            dynamic objects = new DynamicObjectCollection(d, p);

            Assert.Throws<InvalidOperationException>(() =>
                objects.Average()
            );
        }

        [Fact]
        public void One_generic_type_argument()
        {
            var g = new DynamicGraph();
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
        public void Two_generic_type_arguments()
        {
            var g = new DynamicGraph();
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
        public void Three_generic_type_arguments()
        {
            var g = new DynamicGraph();
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
                }
            );
        }
    }
}
