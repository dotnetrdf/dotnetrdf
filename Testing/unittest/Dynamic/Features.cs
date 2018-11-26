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

        public void Converts_objects_to_native_datatypes()
        {
            var g = new Graph();
            g.LoadFromString(@"
@prefix : <urn:> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .

:s
    :p
        :x ,
        _:blank ,
        0E0 ,
        ""0""^^xsd:float ,
        0.0 ,
        false ,
        ""1900-01-01""^^xsd:dateTime ,
        ""P1D""^^xsd:duration ,
        0 ,
        """" ,
        """"^^:datatype ,
        """"@en ,
        (0 1) .
");

            var s = g.CreateUriNode(":s");
            var p = g.CreateUriNode(":p");
            var d = new DynamicNode(s);
            var c = new DynamicObjectCollection(d, p);
            var o = c.ToArray();

            Assert.IsType<DynamicNode>(o[0]);
            Assert.IsType<DynamicNode>(o[1]);
            Assert.IsType<double>(o[2]);
            Assert.IsType<float>(o[3]);
            Assert.IsType<decimal>(o[4]);
            Assert.IsType<bool>(o[5]);
            Assert.IsType<DateTimeOffset>(o[6]);
            Assert.IsType<TimeSpan>(o[7]);
            Assert.IsType<long>(o[8]);
            Assert.IsType<string>(o[9]);
            Assert.IsAssignableFrom<ILiteralNode>(o[10]);
            Assert.IsAssignableFrom<ILiteralNode>(o[11]);
            Assert.IsType<DynamicCollectionList>(o[12]);
        }
    }
}
