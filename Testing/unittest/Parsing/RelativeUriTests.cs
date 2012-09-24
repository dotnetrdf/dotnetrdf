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
        [TestMethod]
        public void ParsingRelativeUriAppDefinedRdfXml()
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

            //Predicate should get a relative URI which is a File URI
            Uri property = ((IUriNode)t.Predicate).Uri;
            Assert.IsTrue(property.IsFile);
        }

        [TestMethod, ExpectedException(typeof(RdfParseException))]
        public void ParsingRelativeUriUndefinedRdfXml()
        {
            //This invocation fails because when invoking the parser directly
            //the Base URI is not set to the file URI

            Graph g = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(g, "rdfxml-relative-uri.rdf");
        }
    }
}
