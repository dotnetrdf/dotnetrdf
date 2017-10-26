/*
dotNetRDF is free and open source software licensed under the MIT License

-----------------------------------------------------------------------------

Copyright (c) 2009-2012 dotNetRDF Project (dotnetrdf-developer@lists.sf.net)

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is furnished
to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR 
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using VDS.RDF.Writing;

namespace VDS.RDF.Web
{

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

            Assert.Equal(expected, writer.GetType());
            Assert.Equal(contentType, ctype);
        }

        [Fact]
        public void WebConnegGetSparqlWriterWithAcceptAll()
        {
            this.TestSparqlWriterConneg("*/*", typeof(SparqlXmlWriter), "application/sparql-results+xml");
        }

        [Fact]
        public void WebConnegGetSparqlWriterWithAcceptXml()
        {
            this.TestSparqlWriterConneg("application/sparql-results+xml", typeof(SparqlXmlWriter), "application/sparql-results+xml");
        }

        [Fact]
        public void WebConnegGetSparqlWriterWithAcceptJson()
        {
            this.TestSparqlWriterConneg("application/sparql-results+json", typeof(SparqlJsonWriter), "application/sparql-results+json");
        }

        [Fact]
        public void WebConnegGetSparqlWriterWithAcceptHtml()
        {
            this.TestSparqlWriterConneg("text/html", typeof(SparqlHtmlWriter), "text/html");
        }
    }
}
