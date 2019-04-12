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

// 1. Allowed values for subjects and predicates (see DynamicExtensions.AsUriNode()):
// - empty strings
// - QName strings
// - relative URI strings
// - absolute URI strings
// - relative URIs
// - absolute URIs
// - nodes

// 2. Allowed types for objects (see DynamicExtensions.AsNode(this object value, IGraph graph)):
// - null
// - INode
// - Uri
// - bool
// - byte
// - DateTime
// - DateTimeOffset
// - decimal
// - double
// - float
// - long
// - int
// - string
// - char
// - TimeSpan
// - IEnumerables with items of the above types

// 3. Allowed values for predicateAndObjects
// - null
// - dictionary instances with keys from 1. and values from 2.
// - anonymous class instances with values from 2.
// - custom class instances with public properties with values from 2.

// 4. Allowed types for SPARQL result bindings (see DynamicExtensions.AsNode(this object value)):
// - null
// - INode
// - Uri
// - bool
// - byte
// - DateTime
// - DateTimeOffset
// - decimal
// - double
// - float
// - long
// - int
// - string
// - char
// - TimeSpan

namespace VDS.RDF.Dynamic
{
    using System.Collections.Generic;
    using System.Linq;
    using VDS.RDF;
    using VDS.RDF.Query;
    using Xunit;

    public partial class Examples
    {
        [Fact]
        public void Example1()
        {
            var g = new Graph();
            g.LoadFromFile(@"resources\rvesse.ttl");

            var d = g.AsDynamic(UriFactory.Create("http://www.dotnetrdf.org/people#"), predicateBaseUri: UriFactory.Create("http://xmlns.com/foaf/0.1/"));

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

            var g = new Graph();
            g.BaseUri = UriFactory.Create("http://example.org/");
            g.NamespaceMap.AddNamespace(string.Empty, UriFactory.Create("http://example.org/"));
            g.NamespaceMap.AddNamespace("rdfs", UriFactory.Create("http://www.w3.org/2000/01/rdf-schema#"));

            var d = g.AsDynamic();

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
                        "Example\r\n     \r\nLong Literal"
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
        public void Graph_get_index()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var expected = g.CreateUriNode(UriFactory.Create("urn:s"));

            // See 1. for other key options
            var actual = d["s"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Graph_get_member()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var expected = g.CreateUriNode(UriFactory.Create("urn:s"));

            var actual = d.s;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Graph_set_index()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));

            // See 1. for other key options
            // See 3. for other value options
            d["s"] = new { p = "o" };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Graph_set_member()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));

            // See 3. for other value options
            d.s = new { p = "o" };

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Graph_add()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph { BaseUri = UriFactory.Create("urn:") };
            var d = actual.AsDynamic();

            // See 1. for other key options
            // See 3. for other value options
            d.Add("s", new { p = "o" });

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Graph_contains()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));

            // See 1. for other key options
            // See 3. for other value options
            var condition = d.Contains("s", new { p = "o" });

            Assert.True(condition);
        }

        [Fact]
        public void Graph_contains_key()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));

            // See 1. for other key options
            var condition = d.ContainsKey("s");

            Assert.True(condition);
        }

        [Fact]
        public void Graph_remove_subject()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));

            // See 1. for other key options
            d.Remove("s");

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Graph_remove_subject_predicate_object()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));

            // See 1. for other key options
            // See 3. for other value options
            d.Remove("s", new { p = "o" });

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Node_get_index()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var expected = "o".AsEnumerable();
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            var actual = s["p"];

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Node_get_member()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var expected = "o".AsEnumerable();
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            var actual = s.p;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Node_set_index()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            // See 2. for other value options
            s["p"] = "o";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Node_set_member()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 2. for other value options
            s.p = "o";

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Node_add()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            // See 2. for other value options
            s.Add("p", "o");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Node_contains()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            // See 2. for other value options
            var condition = s.Contains("p", "o");

            Assert.True(condition);
        }

        [Fact]
        public void Node_contains_key()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            var condition = s.ContainsKey("p");

            Assert.True(condition);
        }

        [Fact]
        public void Node_clear()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            s.Clear();

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Node_remove_predicate()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            s.Remove("p");

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Node_remove_predicate_object()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var s = d.s;

            // See 1. for other key options
            // See 2. for other value options
            s.Remove("p", "o");

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Object_collection_count()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var expected = 1;
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var sp = d.s.p;

            var actual = sp.Count;

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Object_collection_add()
        {
            var expected = new Graph();
            expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var actual = new Graph();
            var d = actual.AsDynamic(UriFactory.Create("urn:"));
            var sp = d.s.p;

            // See 2. for other value options
            sp.Add("o");

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void Object_collection_clear()
        {
            var g = new Graph();
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var sp = d.s.p;

            sp.Clear();

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Object_collection_contains()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var sp = d.s.p;

            // See 2. for other value options
            var condition = sp.Contains("o");

            Assert.True(condition);
        }

        [Fact]
        public void Object_collection_remove()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var sp = d.s.p;

            // See 2. for other value options
            sp.Remove("o");

            Assert.True(g.IsEmpty);
        }

        [Fact]
        public void Object_collection_linq()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var d = g.AsDynamic(UriFactory.Create("urn:"));
            var expected = "o";
            var sp = d.s.p;

            var actual = sp.Single();

            Assert.Equal(expected, actual);
        }

        [Fact]
        public void SparqlResultSet_linq()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var set = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();

            var result = set.Single();

            Assert.IsType<DynamicSparqlResult>(result);
        }

        [Fact]
        public void SparqlResult_get_index()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();
            var result = results.Single();

            var actual = result["o"];

            Xunit.Assert.Equal("o", actual);
        }

        [Fact]
        public void SparqlResult_get_member()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();
            var result = results.Single();

            var actual = result.o;

            Xunit.Assert.Equal("o", actual);
        }

        [Fact]
        public void SparqlResult_set_index()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();
            var expected = new NodeFactory().CreateLiteralNode("o1");

            // See 4. for other value options
            dynamicResult["o"] = "o1";

            Assert.Equal(expected, result["o"]);
        }

        [Fact]
        public void SparqlResult_set_member()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();
            var expected = new NodeFactory().CreateLiteralNode("o1");

            // See 2. for other value options
            dynamicResult.o = "o1";

            Assert.Equal(expected, result["o"]);
        }

        [Fact]
        public void SparqlResult_add()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();
            var expected = new NodeFactory().CreateLiteralNode("y");

            // See 2. for other value options
            dynamicResult.Add("x", "y");

            Assert.Equal(expected, result["x"]);
        }

        [Fact]
        public void SparqlResult_clear()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();

            dynamicResult.Clear();

            Assert.Empty(result);
        }

        [Fact]
        public void SparqlResult_contains_key()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();

            var condition = dynamicResult.ContainsKey("s");

            Assert.True(condition);
        }

        [Fact]
        public void SparqlResult_remove()
        {
            var g = new Graph();
            g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
            var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
            var result = results.Single();
            var dynamicResults = results.AsDynamic();
            var dynamicResult = dynamicResults.Single();

            dynamicResult.Remove("s");

            Assert.False(result.HasValue("s"));
        }
    }
}
