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

            HtmlSchemaWriter writer = new HtmlSchemaWriter();
            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            writer.Save(g, strWriter);

            Console.WriteLine(strWriter.ToString());

            Assert.IsFalse(strWriter.ToString().Contains("type"), "Should not have documented any classes");
        }
    }
}
