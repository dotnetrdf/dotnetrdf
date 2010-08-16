using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test
{
    [TestClass()]
	public class RdfXmlTests
	{
        [TestMethod()]
        public void RdfXmlAmpersandsTest()
        {
            List<IRdfWriter> writers = new List<IRdfWriter>()
            {
                new RdfXmlTreeWriter(),
                new FastRdfXmlWriter()
            };
            IRdfReader parser = new RdfXmlParser();

            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/ampersandsInRdfXml");
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateUriNode(new Uri("http://example.org/a&b"))));
            g.Assert(new Triple(g.CreateUriNode(), g.CreateUriNode(new Uri("http://example.org/property")), g.CreateLiteralNode("A & B")));

            foreach (IRdfWriter writer in writers)
            {
                try
                {
                    Console.WriteLine(writer.GetType().ToString());
                    String temp = StringWriter.Write(g, writer);
                    Console.WriteLine(temp);
                    Graph h = new Graph();
                    StringParser.Parse(h, temp);
                    Assert.AreEqual(g, h, "Graphs should be equal");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    TestTools.ReportError("Error", ex, true);
                }
            }
        }
	}
}
