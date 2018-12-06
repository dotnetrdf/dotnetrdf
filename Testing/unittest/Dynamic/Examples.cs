namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;
    using Xunit;

    public class Examples
    {
        [Fact]
        public void Example1()
        {
            var g = new DynamicGraph(
                subjectBaseUri: UriFactory.Create("http://www.dotnetrdf.org/people#"),
                predicateBaseUri: UriFactory.Create("http://xmlns.com/foaf/0.1/"));
            g.LoadFromFile(@"resources\rvesse.ttl");

            dynamic d = g;

            var rvesse = d.rvesse;
            var onlineAccount = rvesse.OnlineAccount.Single();
            var accountName = onlineAccount.accountName.Single();
            var currentProjects = rvesse.currentProject;
            var mbox = rvesse.mbox;
            var hex_id = rvesse["wot:hasKey"].Single()["wot:hex_id"].Single();

            Assert.Equal(g.CreateUriNode(UriFactory.Create("http://www.dotnetrdf.org/people#rvesse")), rvesse);
            Assert.IsAssignableFrom<IBlankNode>(onlineAccount);
            Assert.Equal("RobVesse", accountName);
            Assert.Equal(2, currentProjects.Count());
            Assert.Equal(
                new[] {
                    "mailto:rav08r@ecs.soton.ac.uk",
                    "mailto:rvesse@apache.org",
                    "mailto:rvesse@cray.com",
                    "mailto:rvesse@dotnetrdf.org",
                    "mailto:rvesse@vdesign-studios.com",
                    "mailto:rvesse@yarcdata.com"
                },
                ((DynamicObjectCollection)mbox).Select(mb => mb.ToString())
            );
            Assert.Equal("6E2497EB", hex_id);
        }

        [Fact]
        public void Example2()
        {
            var expected = new Graph();
            expected.LoadFromFile(@"resources\Turtle.ttl");

            var g = new DynamicGraph();
            g.BaseUri = UriFactory.Create("http://example.org/");
            g.NamespaceMap.AddNamespace("", UriFactory.Create("http://example.org/"));
            g.NamespaceMap.AddNamespace("rdfs", UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#"));

            dynamic d = g;

            d.one = new Dictionary<object, object> {
                {
                    "rdf:type",
                    d.CreateUriNode("rdfs:Class")
                },
                {
                    "rdfs:label",
                    new[] {
                        "Example Literal",
                        @"Example
     
Long Literal"
                    }
                },
            };

            d.two = new
            {
                age = new object[] {
                    23,
                    23.6m
                }
            };

            d.three = new
            {
                name = new object[] {
                    "Name",
                    d.CreateLiteralNode("Nom", "fr")
                }
            };

            d.four = new
            {
                male = false,
                female = true
            };

            d.five = new
            {
                property = d.CreateUriNode(":value")
            };

            d.six = new
            {
                property = new object[] {
                    d.CreateLiteralNode("value", d.CreateUriNode(":customDataType").Uri),
                    // Can't use DateTime because DateTimeNode default serialization is not as source
                    d.CreateLiteralNode("2009-08-25T13:15:00+01:00", d.CreateUriNode("xsd:dateTime").Uri)
                }
            };

            d[d.CreateBlankNode()]["rdf:type"] = d.CreateUriNode(":BlankNode");

            var blankNodeCollection = d[d.CreateBlankNode()];
            blankNodeCollection.property = new[] {
                d.CreateUriNode(":value"),
                d.CreateUriNode(":otherValue")
            };
            blankNodeCollection["rdf:type"] = d.CreateUriNode(":BlankNodeCollection");

            var collection = g.AssertList(new[] {
                g.CreateUriNode(":item1"),
                g.CreateUriNode(":item2"),
                g.CreateUriNode(":item3")
            });
            d[collection]["rdf:type"] = d.CreateUriNode(":Collection");

            Assert.Equal<IGraph>(expected, g);
        }
    }
}
