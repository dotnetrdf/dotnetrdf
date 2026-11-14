/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

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

using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Resolvers;
using System.Xml.Schema;
using Xunit;

namespace VDS.RDF.Writing;

public class GraphMLTests : IClassFixture<GraphMLFixture>
{
    private GraphMLFixture fixture;

    public GraphMLTests(GraphMLFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void Produces_a_graph_element_per_graph()
    {
        foreach (var graph in fixture.Input.Graphs)
        {
            var graphElement = fixture.GraphElementByBaseUri((graph.Name as IUriNode)?.Uri);

            Assert.NotNull(graphElement);
        }
    }

    [Fact]
    public void Produces_an_edge_element_per_triple()
    {
        foreach (var graph in fixture.Input.Graphs)
        {
            var graphElement = fixture.GraphElementByBaseUri((graph.Name as IUriNode)?.Uri);

            var expected = graph.Triples.Count();
            var actual = graphElement.Elements(XName.Get(GraphMLSpecsHelper.Edge, GraphMLSpecsHelper.NS)).Count();

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Produces_a_node_element_per_node()
    {
        foreach (var graph in fixture.Input.Graphs)
        {
            var graphElement = fixture.GraphElementByBaseUri((graph.Name as IUriNode)?.Uri);

            var expected = graph.Nodes.Count();
            var actual = graphElement.Elements(XName.Get(GraphMLSpecsHelper.Node, GraphMLSpecsHelper.NS)).Count();

            Assert.Equal(expected, actual);
        }
    }

    [Fact]
    public void Collapses_literals_by_default()
    {
        var s = new TripleStore();
        var g = new Graph();
        g.LoadFromString(@"
_:s1 <urn:p> ""o"" .
_:s2 <urn:p> ""o"" .
");
        s.Add(g);

        var x = new XDocument();
        using (var outputWriter = x.CreateWriter())
        {
            new GraphMLWriter().Save(s, outputWriter);
        }

        var graphElement = x.Descendants(XName.Get(GraphMLSpecsHelper.Graph, GraphMLSpecsHelper.NS)).Single();
        var expected = 3;
        var actual = graphElement.Elements(XName.Get(GraphMLSpecsHelper.Node, GraphMLSpecsHelper.NS)).Count();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Doesnt_collapses_literals_if_required()
    {
        var s = new TripleStore();
        var g = new Graph();
        g.LoadFromString(@"
_:s1 <urn:p> ""o"" .
_:s2 <urn:p> ""o"" .
");
        s.Add(g);

        var x = new XDocument();
        using (var outputWriter = x.CreateWriter())
        {
            new GraphMLWriter() { CollapseLiterals = false }.Save(s, outputWriter);
        }

        var graphElement = x.Descendants(XName.Get(GraphMLSpecsHelper.Graph, GraphMLSpecsHelper.NS)).Single();
        var expected = 4;
        var actual = graphElement.Elements(XName.Get(GraphMLSpecsHelper.Node, GraphMLSpecsHelper.NS)).Count();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Output_conforms_to_XSD()
    {
        ValidateGraphML(fixture.Output.ToString());
    }

    [Fact]
    public void Schema_rejects_graph_without_edge_default()
    {
        var invalidGraph = $@"<graphml xmlns=""{GraphMLSpecsHelper.NS}""><graph /></graphml>";

        Assert.Throws<XmlSchemaValidationException>(() => ValidateGraphML(invalidGraph));
    }

    private static void ValidateGraphML(string graphML)
    {
        var settings = new XmlReaderSettings
        {
            ValidationType = ValidationType.Schema,
            Schemas = GraphMLSchemaSet()
        };
        settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
        settings.ValidationEventHandler += (_, e) => throw e.Exception;

        using var reader = XmlReader.Create(new StringReader(graphML), settings);
        while (reader.Read())
        {
        }
    }

    private static XmlSchemaSet GraphMLSchemaSet()
    {
        var resources = Path.Combine(AppContext.BaseDirectory, "resources");
        var resolver = new XmlPreloadedResolver(XmlKnownDtds.None);
        var structurePath = Path.Combine(resources, "graphml-structure.xsd");
        resolver.Add(new Uri(structurePath), File.ReadAllBytes(structurePath));
        resolver.Add(new Uri("https://www.w3.org/XML/2008/06/xlink.xsd"),
            File.ReadAllBytes(Path.Combine(resources, "xlink.xsd")));
        resolver.Add(new Uri("http://www.w3.org/2001/xml.xsd"),
            File.ReadAllBytes(Path.Combine(resources, "xml.xsd")));

        var schemas = new XmlSchemaSet { XmlResolver = resolver };
        schemas.ValidationEventHandler += (_, e) => throw e.Exception;
        using var reader = XmlReader.Create(Path.Combine(resources, "graphml.xsd"));
        schemas.Add(GraphMLSpecsHelper.NS, reader);
        schemas.Compile();
        return schemas;
    }
}
