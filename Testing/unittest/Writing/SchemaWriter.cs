using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Writing
{
    [TestClass]
    public class SchemaWriter
    {
        [TestMethod]
        public void WritingHtmlSchemaWriter()
        {
            //Load the Graph from within the Assembly
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StreamReader(Assembly.GetAssembly(typeof(IGraph)).GetManifestResourceStream("VDS.RDF.Configuration.configuration.ttl"), Encoding.UTF8));

            //Now generate the HTML file
            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            writer.Save(g, "configSchema.html");
        }

        [TestMethod]
        public void WritingHtmlSchemaWriterAnonClasses()
        {
            //Create an example Graph
            Graph g = new Graph();
            g.Assert(g.CreateBlankNode(), g.CreateUriNode("rdf:type"), g.CreateUriNode("rdfs:class"));

            TestTools.ShowGraph(g);

            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Contains("type"), "Should not have documented any classes");
        }

        [TestMethod]
        public void WritingHtmlSchemaWriterUnionOfRanges()
        {
            //Create an example Graph
            Graph g = new Graph();
            g.NamespaceMap.AddNamespace("ex", UriFactory.Create("http://example.org/"));
            g.NamespaceMap.AddNamespace("owl", UriFactory.Create(NamespaceMapper.OWL));
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

            TestTools.ShowGraph(g);

            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.IsTrue(strWriter.ToString().Contains("ex:one"), "Should have documented ex:one as a range");
            Assert.IsTrue(strWriter.ToString().Contains("ex:two"), "Should have documented ex:two as a range");
        }
    }
}
