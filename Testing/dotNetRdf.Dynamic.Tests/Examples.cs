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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using VDS.RDF.Query;
using Xunit;

namespace VDS.RDF.Dynamic;

public class Examples
{
    [Fact]
    public void Example1()
    {
        var g = new Graph();
        g.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));

        dynamic d = g.AsDynamic(UriFactory.Root.Create("http://www.dotnetrdf.org/people#"), predicateBaseUri: UriFactory.Root.Create("http://xmlns.com/foaf/0.1/"));

        dynamic rvesse = d.rvesse;
        dynamic onlineAccount = rvesse.OnlineAccount.Single();
        dynamic accountName = onlineAccount.accountName.Single();
        dynamic currentProjects = rvesse.currentProject;
        dynamic mbox = rvesse.mbox;
        dynamic hex_id = rvesse["wot:hasKey"].Single()["wot:hex_id"].Single();

        Assert.Equal(g.CreateUriNode(UriFactory.Root.Create("http://www.dotnetrdf.org/people#rvesse")), rvesse);
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
        expected.LoadFromFile(Path.Combine("resources", "Turtle.ttl"));

        var g = new Graph
        {
            BaseUri = UriFactory.Root.Create("http://example.org/")
        };
        g.NamespaceMap.AddNamespace(string.Empty, UriFactory.Root.Create("http://example.org/"));
        g.NamespaceMap.AddNamespace("rdfs", UriFactory.Root.Create("http://www.w3.org/2000/01/rdf-schema#"));

        dynamic d = g.AsDynamic();

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
                    String.Format("Example{0}     {0}Long Literal", Environment.NewLine)
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

        dynamic blankNodeCollection = d[d.CreateBlankNode()];
        blankNodeCollection.property = new[]
        {
            d.CreateUriNode(":value"),
            d.CreateUriNode(":otherValue")
        };
        blankNodeCollection["rdf:type"] = d.CreateUriNode(":BlankNodeCollection");

        INode collection = g.AssertList(new[]
        {
            g.CreateUriNode(":item1"),
            g.CreateUriNode(":item2"),
            g.CreateUriNode(":item3")
        });
        d[collection]["rdf:type"] = d.CreateUriNode(":Collection");
        
        Assert.Equal(expected, g);
    }

    [Fact]
    public void Example3()
    {
        var expected = new Graph();
        expected.LoadFromFile(Path.Combine("resources", "rvesse.ttl"));
        var actual = new Graph();
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("http://www.dotnetrdf.org/people#"));

        var rvesse = new FoafPerson(d.rvesse, actual);
        rvesse.Names.Add("Rob Vesse");
        rvesse.FirstNames.Add("Rob");
        rvesse.LastNames.Add("Vesse");
        rvesse.Homepages.Add(UriFactory.Root.Create("http://www.dotnetrdf.org/"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rav08r@ecs.soton.ac.uk"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rvesse@apache.org"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rvesse@cray.com"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rvesse@dotnetrdf.org"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rvesse@vdesign-studios.com"));
        rvesse.Emails.Add(UriFactory.Root.Create("mailto:rvesse@yarcdata.com"));

        var account = new FoafAccount(d.CreateBlankNode(), actual);
        account.Names.Add("RobVesse");
        account.Profiles.Add(UriFactory.Root.Create("http://twitter.com/RobVesse"));
        account.Services.Add(UriFactory.Root.Create("http://twitter.com/"));
        rvesse.Accounts.Add(account);

        var jena = new DoapProject(d.CreateBlankNode(), actual);
        jena.Names.Add("Apache Jena");
        jena.Homepages.Add(UriFactory.Root.Create("http://incubator.apache.org/jena"));
        rvesse.Projects.Add(jena);

        var dnr = new DoapProject(d.CreateBlankNode(), actual);
        dnr.Names.Add("dotNetRDF");
        dnr.Homepages.Add(UriFactory.Root.Create("http://www.dotnetrdf.org/"));
        rvesse.Projects.Add(dnr);

        var key = new WotKey(d.CreateBlankNode(), actual);
        key.Ids.Add("6E2497EB");
        key.Fingerprints.Add("7C4C 2916 BF74 E242 53BC  C5B7 764D FB13 6E24 97EB");
        rvesse.Keys.Add(key);

        Assert.Equal(expected, actual);
    }

    private class FoafPerson : DynamicNode
    {
        public FoafPerson(INode node, IGraph graph)
            : base(node, graph, UriFactory.Root.Create("http://xmlns.com/foaf/0.1/"))
            => Add("rdf:type", UriFactory.Root.Create("http://xmlns.com/foaf/0.1/Person"));

        public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");

        public ICollection<string> FirstNames => new DynamicObjectCollection<string>(this, "givenname");

        public ICollection<string> LastNames => new DynamicObjectCollection<string>(this, "family_name");

        public ICollection<Uri> Homepages => new DynamicObjectCollection<Uri>(this, "homepage");

        public ICollection<Uri> Emails => new DynamicObjectCollection<Uri>(this, "mbox");

        public ICollection<FoafAccount> Accounts => new DynamicObjectCollection<FoafAccount>(this, "OnlineAccount");

        public ICollection<DoapProject> Projects => new DynamicObjectCollection<DoapProject>(this, "currentProject");

        public new ICollection<WotKey> Keys => new DynamicObjectCollection<WotKey>(this, "http://xmlns.com/wot/0.1/hasKey");
    }

    private class FoafAccount : DynamicNode
    {
        public FoafAccount(INode node, IGraph graph)
            : base(node, graph, UriFactory.Root.Create("http://xmlns.com/foaf/0.1/")) { }

        public ICollection<string> Names => new DynamicObjectCollection<string>(this, "accountName");

        public ICollection<Uri> Profiles => new DynamicObjectCollection<Uri>(this, "accountProfilePage");

        public ICollection<Uri> Services => new DynamicObjectCollection<Uri>(this, "accountServiceHomepage");
    }

    private class DoapProject : DynamicNode
    {
        public DoapProject(INode node, IGraph graph)
            : base(node, graph, UriFactory.Root.Create("http://www.web-semantics.org/ns/pm#"))
            => Add("rdf:type", UriFactory.Root.Create("http://usefulinc.com/ns/doap#Project"));

        public ICollection<string> Names => new DynamicObjectCollection<string>(this, "name");

        public ICollection<Uri> Homepages => new DynamicObjectCollection<Uri>(this, "homepage");
    }

    private class WotKey : DynamicNode
    {
        public WotKey(INode node, IGraph graph)
            : base(node, graph, UriFactory.Root.Create("http://xmlns.com/wot/0.1/"))
            => Add("rdf:type", UriFactory.Root.Create("http://xmlns.com/wot/0.1/PubKey"));

        public ICollection<string> Ids => new DynamicObjectCollection<string>(this, "hex_id");

        public ICollection<string> Fingerprints => new DynamicObjectCollection<string>(this, "fingerprint");
    }

    [Fact]
    public void Graph_get_index()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        IUriNode expected = g.CreateUriNode(UriFactory.Root.Create("urn:s"));

        // See 1. for other key options
        dynamic actual = d["s"];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Graph_get_member()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        IUriNode expected = g.CreateUriNode(UriFactory.Root.Create("urn:s"));

        dynamic actual = d.s;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Graph_set_index()
    {
        var expected = new Graph();
        expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var actual = new Graph();
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));

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
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));

        // See 3. for other value options
        d.s = new { p = "o" };

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Graph_add()
    {
        var expected = new Graph();
        expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var actual = new Graph { BaseUri = UriFactory.Root.Create("urn:") };
        dynamic d = actual.AsDynamic();

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
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));

        // See 1. for other key options
        // See 3. for other value options
        dynamic condition = d.Contains("s", new { p = "o" });

        Assert.True(condition);
    }

    [Fact]
    public void Graph_contains_key()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));

        // See 1. for other key options
        dynamic condition = d.ContainsKey("s");

        Assert.True(condition);
    }

    [Fact]
    public void Graph_remove_subject()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));

        // See 1. for other key options
        d.Remove("s");

        Assert.True(g.IsEmpty);
    }

    [Fact]
    public void Graph_remove_subject_predicate_object()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));

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
        IEnumerable<string> expected = "o".AsEnumerable();
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        // See 1. for other key options
        dynamic actual = s["p"];

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Node_get_member()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        IEnumerable<string> expected = "o".AsEnumerable();
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        dynamic actual = s.p;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Node_set_index()
    {
        var expected = new Graph();
        expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var actual = new Graph();
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

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
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

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
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

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
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        // See 1. for other key options
        // See 2. for other value options
        dynamic condition = s.Contains("p", "o");

        Assert.True(condition);
    }

    [Fact]
    public void Node_contains_key()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        // See 1. for other key options
        dynamic condition = s.ContainsKey("p");

        Assert.True(condition);
    }

    [Fact]
    public void Node_clear()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        s.Clear();

        Assert.True(g.IsEmpty);
    }

    [Fact]
    public void Node_remove_predicate()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

        // See 1. for other key options
        s.Remove("p");

        Assert.True(g.IsEmpty);
    }

    [Fact]
    public void Node_remove_predicate_object()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic s = d.s;

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
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic sp = d.s.p;

        dynamic actual = sp.Count;

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Object_collection_add()
    {
        var expected = new Graph();
        expected.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var actual = new Graph();
        dynamic d = actual.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic sp = d.s.p;

        // See 2. for other value options
        sp.Add("o");

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Object_collection_clear()
    {
        var g = new Graph();
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic sp = d.s.p;

        sp.Clear();

        Assert.True(g.IsEmpty);
    }

    [Fact]
    public void Object_collection_contains()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic sp = d.s.p;

        // See 2. for other value options
        dynamic condition = sp.Contains("o");

        Assert.True(condition);
    }

    [Fact]
    public void Object_collection_remove()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        dynamic sp = d.s.p;

        // See 2. for other value options
        sp.Remove("o");

        Assert.True(g.IsEmpty);
    }

    [Fact]
    public void Object_collection_linq()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic d = g.AsDynamic(UriFactory.Root.Create("urn:"));
        var expected = "o";
        dynamic sp = d.s.p;

        dynamic actual = sp.Single();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SparqlResultSet_linq()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic set = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();

        dynamic result = set.Single();

        Assert.IsType<DynamicSparqlResult>(result);
    }

    [Fact]
    public void SparqlResult_get_index()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic results = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();
        dynamic result = results.Single();

        dynamic actual = result["o"];

        Assert.Equal("o", actual);
    }

    [Fact]
    public void SparqlResult_get_member()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        dynamic results = ((SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }")).AsDynamic();
        dynamic result = results.Single();

        dynamic actual = result.o;

        Assert.Equal("o", actual);
    }

    [Fact]
    public void SparqlResult_set_index()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
        ISparqlResult result = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();
        ILiteralNode expected = new NodeFactory(new NodeFactoryOptions()).CreateLiteralNode("o1");

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
        ISparqlResult result = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();
        ILiteralNode expected = new NodeFactory(new NodeFactoryOptions()).CreateLiteralNode("o1");

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
        ISparqlResult result = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();
        ILiteralNode expected = new NodeFactory(new NodeFactoryOptions()).CreateLiteralNode("y");

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
        ISparqlResult result = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();

        dynamicResult.Clear();

        Assert.Empty(result);
    }

    [Fact]
    public void SparqlResult_contains_key()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
        ISparqlResult _ = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();

        dynamic condition = dynamicResult.ContainsKey("s");

        Assert.True(condition);
    }

    [Fact]
    public void SparqlResult_remove()
    {
        var g = new Graph();
        g.LoadFromString(@"<urn:s> <urn:p> ""o"" .");
        var results = (SparqlResultSet)g.ExecuteQuery(@"SELECT * WHERE { ?s ?p ?o }");
        ISparqlResult result = results.Single();
        dynamic dynamicResults = results.AsDynamic();
        dynamic dynamicResult = dynamicResults.Single();

        dynamicResult.Remove("s");

        Assert.False(result.HasValue("s"));
    }
}
