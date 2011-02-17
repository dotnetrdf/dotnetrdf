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
            VDS.RDF.Writing.HtmlSchemaWriter writer = new HtmlSchemaWriter();
            writer.Save(g, "configSchema.html");
        }
    }
}
