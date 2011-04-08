using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;
using VDS.RDF.Parsing.Handlers;
using VDS.RDF.Writing.Formatting;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class StringReaderEncodingTests
    {
        [TestMethod]
        public void ParsingStringReaderEncoding()
        {
            String test = "<http://example.org/subject> <http://example.org/predicate> \"" + (char)32769 + "\" . ";

            TurtleParser parser = new TurtleParser();
            Graph g = new Graph();
            parser.Load(g, new StringReader(test));
            g.SaveToFile("encoding.ttl");

            Graph h = new Graph();
            parser.Load(h, "encoding.ttl");

            Assert.AreEqual(g, h);
        }
    }
}
