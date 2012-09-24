using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class NamespaceTests
    {
        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingNamespaceNotDeclaredGraphInvokeRdfXml()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("rdfxml-namespace-declaration.rdf", parser);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingNamespaceNotDeclaredParserInvokeRdfXml()
        {
            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, "rdfxml-namespace-declaration.rdf");
        }
    }
}
