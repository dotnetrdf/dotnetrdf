using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Parsing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class NamespaceTests
    {
        private const String TurtleExample = @"[] a <relative> .";

        [TestMethod]
        public void ParsingRelativeUriAppBaseRdfXml1()
        {
            //This invocation succeeds because when invoking via the FileLoader
            //the Base URI will be set to the file URI

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            g.LoadFromFile("rdfxml-relative-uri.rdf", parser);

            //Expect a non-empty grpah with a single triple
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Object should get it's relative URI resolved into
            //a File URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.IsTrue(obj.IsFile);
            Assert.AreEqual("relative", obj.Segments[obj.Segments.Length - 1]);
        }

        [TestMethod]
        public void ParsingRelativeUriAppBaseRdfXml2()
        {
            //This invocation succeeds because when invoking because
            //we manually set the Base URI prior to invoking the parser

            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org");
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, "rdfxml-relative-uri.rdf");

            //Expect a non-empty grpah with a single triple
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Object should get it's relative URI resolved into
            //the correct HTTP URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.AreEqual("http", obj.Scheme);
            Assert.AreEqual("example.org", obj.Host);
            Assert.AreEqual("relative", obj.Segments[1]);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingRelativeUriNoBaseRdfXml()
        {
            //This invocation fails because when invoking the parser directly
            //the Base URI is not set to the file URI

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, "rdfxml-relative-uri.rdf");
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingRelativeUriNoBaseTurtle()
        {
            //This invocation fails because there is no Base URI to
            //resolve against
            Graph g = new Graph();
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(TurtleExample));
        }

        [TestMethod]
        public void ParsingRelativeUriAppBaseTurtle()
        {
            //This invocation succeeds because we define a Base URI
            //resolve against
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org");
            TurtleParser parser = new TurtleParser();
            parser.Load(g, new StringReader(TurtleExample));

            //Expect a non-empty grpah with a single triple
            Assert.IsFalse(g.IsEmpty);
            Assert.AreEqual(1, g.Triples.Count);
            Triple t = g.Triples.First();

            //Predicate should get it's relative URI resolved into
            //the correct HTTP URI
            Uri obj = ((IUriNode)t.Object).Uri;
            Assert.AreEqual("http", obj.Scheme);
            Assert.AreEqual("example.org", obj.Host);
            Assert.AreEqual("relative", obj.Segments[1]);
        }
    }
}
