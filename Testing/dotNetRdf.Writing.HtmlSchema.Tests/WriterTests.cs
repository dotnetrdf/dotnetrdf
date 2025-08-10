using System;
using System.IO;
using System.Reflection;
using System.Text;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;
using Xunit;
using StringWriter = VDS.RDF.Writing.StringWriter;

namespace VDS.RDF;

public class WriterTests
{
    private readonly ITestOutputHelper _output;

    public WriterTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void TextWriterCanBeLeftOpen()
    {
        var g = new Graph();
        var writer = new System.IO.StringWriter();
        var rdfWriter = new HtmlSchemaWriter();
        rdfWriter.Save(g, writer, true);
        writer.Write("\n"); // This should not throw because the writer is still open
        rdfWriter.Save(g, writer);
        Assert.Throws<ObjectDisposedException>(() => writer.Write("\n"));
    }

    [Fact]
    public void WritingHtmlSchemaWriter()
    {
        //Load the Graph from within the Assembly
        var g = new Graph();
        var parser = new TurtleParser();
        parser.Load(g, new StreamReader(typeof(IGraph).GetTypeInfo().Assembly.GetManifestResourceStream("VDS.RDF.Configuration.configuration.ttl"), Encoding.UTF8));

        //Now generate the HTML file
        var writer = new HtmlSchemaWriter();
        writer.Save(g, "configSchema.html");
    }

    [Fact]
    public void WritingHtmlSchemaWriterAnonClasses()
    {
        //Create an example Graph
        var g = new Graph();
        g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("rdfs:class"));

        TestTools.ShowGraph(g, _output);

        var writer = new HtmlSchemaWriter();
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);

        _output.WriteLine(strWriter.ToString());

        Assert.False(strWriter.ToString().Contains("type"), "Should not have documented any classes");
    }

    [Fact]
    public void WritingHtmlSchemaWriterUnionOfRanges()
    {
        //Create an example Graph
        var g = new Graph();
        g.NamespaceMap.AddNamespace("ex", UriFactory.Root.Create("http://example.org/"));
        g.NamespaceMap.AddNamespace("owl", UriFactory.Root.Create(NamespaceMapper.OWL));
        INode testProperty = g.CreateUriNode("ex:property");
        INode rdfType = g.CreateUriNode("rdf:type");
        INode rdfProperty = g.CreateUriNode("rdf:Property");
        INode rdfsRange = g.CreateUriNode("rdfs:range");
        INode union = g.CreateBlankNode();
        INode unionOf = g.CreateUriNode("owl:unionOf");
        INode testItem1 = g.CreateUriNode("ex:one");
        INode testItem2 = g.CreateUriNode("ex:two");

        g.Assert(testProperty, rdfType, rdfProperty);
        g.Assert(testProperty, rdfsRange, union);
        g.Assert(union, unionOf, g.AssertList(new INode[] { testItem1, testItem2 }));

        TestTools.ShowGraph(g, _output);

        var writer = new HtmlSchemaWriter();
        var strWriter = new System.IO.StringWriter();
        writer.Save(g, strWriter);

        _output.WriteLine(strWriter.ToString());

        Assert.True(strWriter.ToString().Contains("ex:one"), "Should have documented ex:one as a range");
        Assert.True(strWriter.ToString().Contains("ex:two"), "Should have documented ex:two as a range");
    }
    
    [Fact]
    public void ItCanWriteNodesWithNoFragment()
    {
        var graph = new Graph();
        graph.LoadFromString("<http://example.org/MyClass> a <http://www.w3.org/2002/07/owl#Class> .", new TurtleParser());
        var html = StringWriter.Write(graph, new HtmlSchemaWriter());
        Assert.True(html.Contains("MyClass"), "Should have documented MyClass as a class");
    }
}
