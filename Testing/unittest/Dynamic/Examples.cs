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
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
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
                new[]
                {
                    "mailto:rav08r@ecs.soton.ac.uk",
                    "mailto:rvesse@apache.org",
                    "mailto:rvesse@cray.com",
                    "mailto:rvesse@dotnetrdf.org",
                    "mailto:rvesse@vdesign-studios.com",
                    "mailto:rvesse@yarcdata.com"
                },
                ((IEnumerable<object>)mbox).Select(mb => mb.ToString()));
            Assert.Equal("6E2497EB", hex_id);
        }

        [Fact]
        public void Example2()
        {
            var expected = new Graph();
            expected.LoadFromFile(@"resources\Turtle.ttl");

            var g = new DynamicGraph();
            g.BaseUri = UriFactory.Create("http://example.org/");
            g.NamespaceMap.AddNamespace(string.Empty, UriFactory.Create("http://example.org/"));
            g.NamespaceMap.AddNamespace("rdfs", UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#"));

            dynamic d = g;

            d.one = new Dictionary<object, object>
            {
                {
                    "rdf:type",
                    d.CreateUriNode("rdfs:Class")
                },
                {
                    "rdfs:label",
                    new[]
                    {
                        "Example Literal",
                        @"Example
     
Long Literal"
                    }
                },
            };

            d.two = new
            {
                age = new object[]
                {
                    23,
                    23.6m
                }
            };

            d.three = new
            {
                name = new object[]
                {
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
                property = new object[]
                {
                    d.CreateLiteralNode("value", d.CreateUriNode(":customDataType").Uri),
                    
                    // Can't use DateTime because DateTimeNode default serialization is not as source
                    d.CreateLiteralNode("2009-08-25T13:15:00+01:00", d.CreateUriNode("xsd:dateTime").Uri)
                }
            };

            d[d.CreateBlankNode()]["rdf:type"] = d.CreateUriNode(":BlankNode");

            var blankNodeCollection = d[d.CreateBlankNode()];
            blankNodeCollection.property = new[]
            {
                d.CreateUriNode(":value"),
                d.CreateUriNode(":otherValue")
            };
            blankNodeCollection["rdf:type"] = d.CreateUriNode(":BlankNodeCollection");

            var collection = g.AssertList(new[]
            {
                g.CreateUriNode(":item1"),
                g.CreateUriNode(":item2"),
                g.CreateUriNode(":item3")
            });
            d[collection]["rdf:type"] = d.CreateUriNode(":Collection");

            Assert.Equal<IGraph>(expected, g);
        }

        [Fact]
        public void Graph_get_index_with_dynamic_node()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));

            Assert.Equal(
                s,
                d[s.AsDynamic()]);
        }

        [Fact]
        public void Graph_get_index_with_node()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = g.CreateUriNode(UriFactory.Create("urn:s"));

            Assert.Equal(
                s,
                d[s]);
        }

        [Fact]
        public void Graph_get_index_with_absolute_uri()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d[UriFactory.Create("urn:s")]);
        }

        [Fact]
        public void Graph_get_index_with_relative_uri()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d[new Uri("s", UriKind.Relative)]);
        }

        [Fact]
        public void Graph_get_index_with_absolute_uri_string()
        {
            var g = new Graph();
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d["urn:s"]);
        }

        [Fact]
        public void Graph_get_index_with_relative_uri_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d["s"]);
        }

        [Fact]
        public void Graph_get_index_with_hash_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("http://example.com#") };
            g.LoadFromString("<http://example.com#s> <http://example.com#p> <http://example.com#o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("http://example.com#s")),
                d["s"]);
        }

        [Fact]
        public void Graph_get_index_with_qname()
        {
            var g = new Graph();
            g.NamespaceMap.AddNamespace("u", UriFactory.Create("urn:"));
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d["u:s"]);
        }

        [Fact]
        public void Graph_get_index_with_empty_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:s") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d[""]);
        }

        [Fact]
        public void Graph_get_member()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:s")),
                d.s);
        }

        [Fact]
        public void Graph_set_index_with_custom_class()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d["s"] = new C
            {
                p1 = "o1",
                p2 = new object[]
                {
                    "o2",
                    0,
                    true,
                    UriFactory.Create("urn:o3")
                },
                p3 = g.CreateBlankNode()
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_index_with_dictionary()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d["s"] = new Dictionary<object, object>
            {
                {
                    "p1",
                    "o1"
                },
                {
                    UriFactory.Create("urn:p2"),
                    new object[]
                    {
                        "o2",
                        0,
                        true,
                        UriFactory.Create("urn:o3")
                    }
                },
                {
                    g.CreateUriNode(UriFactory.Create("urn:p3")),
                    g.CreateBlankNode()
                }
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_index_with_anonymous_class()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d["s"] = new
            {
                p1 = "o1",
                p2 = new object[]
                {
                    "o2",
                    0,
                    true,
                    UriFactory.Create("urn:o3")
                },
                p3 = g.CreateBlankNode()
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_index_with_null()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            d["s"] = null;

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Graph_set_member_with_custom_class()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d.s = new C
            {
                p1 = "o1",
                p2 = new object[]
                {
                    "o2",
                    0,
                    true,
                    UriFactory.Create("urn:o3")
                },
                p3 = g.CreateBlankNode()
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_member_with_dictionary()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d.s = new Dictionary<object, object>
            {
                {
                    "p1",
                    "o1"
                },
                {
                    UriFactory.Create("urn:p2"),
                    new object[]
                    {
                        "o2",
                        0,
                        true,
                        UriFactory.Create("urn:o3")
                    }
                },
                {
                    g.CreateUriNode(UriFactory.Create("urn:p3")),
                    g.CreateBlankNode()
                }
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_member_with_anonymous_class()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
@prefix : <urn:> .

:s
    :p1 ""o1"" ;
    :p2 ""o2"", 0, true, :o3 ;
    :p3 [] .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();

            d.s = new
            {
                p1 = "o1",
                p2 = new object[]
                {
                    "o2",
                    0,
                    true,
                    UriFactory.Create("urn:o3")
                },
                p3 = g.CreateBlankNode()
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Graph_set_member_with_null()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString(@"<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();

            d.s = null;

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Node_get_index_with_dynamic_node()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s[d.p].Single());
        }

        [Fact]
        public void Node_get_index_with_node()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s[g.CreateUriNode(UriFactory.Create("urn:p"))].Single());
        }

        [Fact]
        public void Node_get_index_with_absolute_uri()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s[UriFactory.Create("urn:p")].Single());
        }

        [Fact]
        public void Node_get_index_with_relative_uri()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s[new Uri("p", UriKind.Relative)].Single());
        }

        [Fact]
        public void Node_get_index_with_absolute_uri_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s["urn:p"].Single());
        }

        [Fact]
        public void Node_get_index_with_relative_uri_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s["p"].Single());
        }

        [Fact]
        public void Node_get_index_with_hash_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("http://example.com#") };
            g.LoadFromString("<http://example.com#s> <http://example.com#p> <http://example.com#o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("http://example.com#o")),
                s["p"].Single());
        }

        [Fact]
        public void Node_get_index_with_qname()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.NamespaceMap.AddNamespace("u", UriFactory.Create("urn:"));
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s["u:p"].Single());
        }

        [Fact]
        public void Node_get_index_with_empty_string()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic(predicateBaseUri: UriFactory.Create("urn:p"));
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s[""].Single());
        }

        [Fact]
        public void Node_get_member()
        {
            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            g.LoadFromString("<urn:s> <urn:p> <urn:o> .");

            var d = g.AsDynamic();
            var s = d.s;

            Assert.Equal(
                g.CreateUriNode(UriFactory.Create("urn:o")),
                s.p.Single());
        }

        [Fact]
        public void Node_set_index_with_single()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();
            var s = d.s;

            s["p"] = "o";

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Node_set_index_with_enumerable()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
<urn:s> <urn:p> <urn:o> .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();
            var s = d.s;

            s["p"] = new object[]
            {
                "o",
                UriFactory.Create("urn:o")
            };

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Node_set_member_with_single()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();
            var s = d.s;

            s.p = "o";

            Assert.Equal(
                expected,
                g);
        }

        [Fact]
        public void Node_set_member_with_enumerable()
        {
            var expected = new Graph();
            expected.LoadFromString(@"
<urn:s> <urn:p> ""o"" .
<urn:s> <urn:p> <urn:o> .
");

            var g = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = g.AsDynamic();
            var s = d.s;

            s.p = new object[]
            {
                "o",
                UriFactory.Create("urn:o")
            };

            Assert.Equal(
                expected,
                g);
        }

        private class C
        {
            public string p1 { get; set; }

            public object[] p2 { get; set; }

            public IBlankNode p3 { get; set; }
        }
    }
}
