namespace VDS.RDF.Dynamic
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class Features
    {
        public void Handles_nested_lists1()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var r = g.GetTriplesWithSubjectPredicate(s, p).Single().Object;
            var l = new DynamicCollectionList(r);

            Assert.Contains("o1", l);
            Assert.Contains("o3", l);
            Assert.DoesNotContain("o2", l);
            Assert.IsAssignableFrom<IList<object>>(l[1]);
            Assert.Contains("o2", l[1] as IList<object>);
        }

        public void Handles_nested_lists2()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s :p (""o1"" (""o2"") ""o3"") .
");

            var d = new DynamicGraph(null, UriFactory.Create("urn:"));
            d["s"] = new
            {
                p = new RdfCollection(
                    "o1" as object,
                    new RdfCollection(
                        "o2"
                    ),
                    "o3"
                )
            };

            Assert.Equal<IGraph>(expected, d);
        }
    }
}
