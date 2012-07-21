/*

Copyright dotNetRDF Project 2009-12
dotnetrdf-develop@lists.sf.net

------------------------------------------------------------------------

This file is part of dotNetRDF.

dotNetRDF is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

dotNetRDF is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with dotNetRDF.  If not, see <http://www.gnu.org/licenses/>.

------------------------------------------------------------------------

dotNetRDF may alternatively be used under the LGPL or MIT License

http://www.gnu.org/licenses/lgpl.html
http://www.opensource.org/licenses/mit-license.php

If these licenses are not suitable for your intended use please contact
us at the above stated email address to discuss alternative
terms.

*/

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
