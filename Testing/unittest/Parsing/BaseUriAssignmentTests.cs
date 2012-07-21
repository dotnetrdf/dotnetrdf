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
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Test.Parsing
{
    [TestClass]
    public class BaseUriAssignmentTests
    {
        private String ShowBaseUri(Uri baseUri)
        {
            if (baseUri == null)
            {
                return "<NULL>";
            }
            else
            {
                return "<" + baseUri.ToString() + ">";
            }
        }

        [TestMethod]
        public void ParsingBaseUriAssignmentFileLoader()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "InferenceTest.ttl");
            Console.WriteLine("Base URI: " + ShowBaseUri(g.BaseUri));
            Assert.IsNotNull(g.BaseUri, "Base URI should not be null");
        }

        [TestMethod]
        public void ParsingBaseUriAssignmentUriLoader()
        {
            int defaultTimeout = Options.UriLoaderTimeout;
            try
            {
                //DBPedia can be slow so up the timeout for this test
                Options.UriLoaderTimeout = 45000;
                Graph g = new Graph();
                UriLoader.Load(g, new Uri("http://dbpedia.org/resource/Ilkeston"));
                Console.WriteLine("Base URI: " + ShowBaseUri(g.BaseUri));
                Assert.IsNotNull(g.BaseUri, "Base URI should not be null");
            }
            finally
            {
                //Remember to reset timeout afterwards
                Options.UriLoaderTimeout = defaultTimeout;
            }
        }

        [TestMethod]
        public void ParsingBaseUriAssignmentRdfXml()
        {
            Graph g = new Graph();
            g.BaseUri = new Uri("http://example.org/RdfXml");

            System.IO.StringWriter strWriter = new System.IO.StringWriter();
            RdfXmlWriter writer = new RdfXmlWriter();
            writer.Save(g, strWriter);

            Console.WriteLine("Original Base URI: " + ShowBaseUri(g.BaseUri));

            Console.WriteLine("Output using RdfXmlWriter:");
            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            Graph h = new Graph();
            RdfXmlParser parser = new RdfXmlParser();
            parser.Load(h, new System.IO.StringReader(strWriter.ToString()));

            Console.WriteLine("Base URI after round-trip using RdfXmlWriter: " + ShowBaseUri(h.BaseUri));
            Assert.IsNotNull(h.BaseUri, "Base URI should not be null");

            strWriter = new System.IO.StringWriter();
            FastRdfXmlWriter fastWriter = new FastRdfXmlWriter();
            fastWriter.Save(g, strWriter);

            Console.WriteLine("Output using FastRdfXmlWriter:");
            Console.WriteLine(strWriter.ToString());
            Console.WriteLine();

            Graph i = new Graph();
            parser.Load(i, new System.IO.StringReader(strWriter.ToString()));

            Console.WriteLine("Base URI after round-trip to FastRdfXmlWriter: " + ShowBaseUri(h.BaseUri));
            Assert.IsNotNull(i.BaseUri, "Base URI should not be null");
        }


    }
}
