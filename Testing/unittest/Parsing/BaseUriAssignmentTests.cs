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
using NUnit.Framework;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    [TestFixture]
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

        [Test]
        public void ParsingBaseUriAssignmentFileLoader()
        {
            Graph g = new Graph();
            FileLoader.Load(g, "resources\\InferenceTest.ttl");
            Console.WriteLine("Base URI: " + ShowBaseUri(g.BaseUri));
            Assert.IsNotNull(g.BaseUri, "Base URI should not be null");
        }

#if !PORTABLE

        [Test]
        public void ParsingBaseUriAssignmentUriLoader()
        {
            if (!TestConfigManager.GetSettingAsBoolean(TestConfigManager.UseRemoteParsing))
            {
                Assert.Inconclusive("Test Config marks Remote Parsing as unavailable, test cannot be run");
            }

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

#endif

        [Test]
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
        }
    }
}
