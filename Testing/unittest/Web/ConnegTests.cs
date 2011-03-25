using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Web
{
    [TestClass]
    public class ConnegTests
    {
        private void TestSparqlWriterConneg(String header, Type expected, String contentType)
        {
            String ctype;
            ISparqlResultsWriter writer = MimeTypesHelper.GetSparqlWriter(header, out ctype);

            Console.WriteLine("Accept Header: " + header);
            Console.WriteLine("Expected Writer: " + expected.Name);
            Console.WriteLine("Actual Writer: " + writer.GetType().Name);
            Console.WriteLine("Expected Content Type: " + contentType);
            Console.WriteLine("Actual Content Type: " + ctype);

            Assert.AreEqual(expected, writer.GetType());
            Assert.AreEqual(contentType, ctype);
        }

        [TestMethod]
        public void WebConnegGetSparqlWriterWithAcceptAll()
        {
            this.TestSparqlWriterConneg("*/*", typeof(SparqlXmlWriter), "application/sparql-results+xml");
        }

        [TestMethod]
        public void WebConnegGetSparqlWriterWithAcceptXml()
        {
            this.TestSparqlWriterConneg("application/sparql-results+xml", typeof(SparqlXmlWriter), "application/sparql-results+xml");
        }

        [TestMethod]
        public void WebConnegGetSparqlWriterWithAcceptJson()
        {
            this.TestSparqlWriterConneg("application/sparql-results+json", typeof(SparqlJsonWriter), "application/sparql-results+json");
        }

        [TestMethod]
        public void WebConnegGetSparqlWriterWithAcceptHtml()
        {
            this.TestSparqlWriterConneg("text/html", typeof(SparqlHtmlWriter), "text/html");
        }
    }
}
