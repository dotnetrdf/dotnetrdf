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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace VDS.RDF.Parsing
{
    public class RdfATests
    {
        [Fact]
        public void ParsingRdfABadSyntax()
        {
            RdfAParser parser = new RdfAParser();
            Graph g = new Graph();
            Console.WriteLine("Tests parsing a file which has much invalid RDFa syntax in it, some triples will be produced (6-8) but most of the triples are wrongly encoded and will be ignored");
            g.BaseUri = new Uri("http://www.wurvoc.org/vocabularies/om-1.6/Kelvin_scale");
            FileLoader.Load(g, "resources\\bad_rdfa.html");

            Console.WriteLine(g.Triples.Count + " Triples");
            foreach (Triple t in g.Triples)
            {
                Console.WriteLine(t.ToString());
            }

            Console.WriteLine();
            CompressingTurtleWriter ttlwriter = new CompressingTurtleWriter(WriterCompressionLevel.High);
            ttlwriter.HighSpeedModePermitted = false;
            ttlwriter.Save(g, "test.ttl");
        }

        [Fact]
        public void ParsingRdfAGoodRelations()
        {
            try
            {
                Options.UriLoaderCaching = false;
                List<String> tests = new List<string>()
                {
                    "gr1",
                    "gr2",
                    "gr3"
                };

                FileLoader.Warning += TestTools.WarningPrinter;

                foreach (String test in tests)
                {
                    Console.WriteLine("Test '" + test + "'");
                    Console.WriteLine();

                    Graph g = new Graph();
                    g.BaseUri = new Uri("http://example.org/goodrelations");
                    Graph h = new Graph();
                    h.BaseUri = g.BaseUri;

                    Console.WriteLine("Graph A Warnings:");
                    FileLoader.Load(g, string.Format("resources\\{0}.xhtml", test));
                    Console.WriteLine();
                    Console.WriteLine("Graph B Warnings:");
                    FileLoader.Load(h, string.Format("resources\\{0}b.xhtml", test));
                    Console.WriteLine();

                    Console.WriteLine("Graph A (RDFa 1.0)");
                    TestTools.ShowGraph(g);
                    Console.WriteLine();
                    Console.WriteLine("Graph B (RDFa 1.1)");
                    TestTools.ShowGraph(h);
                    Console.WriteLine();

                    Assert.Equal(g, h);
                }
            }
            finally
            {
                Options.UriLoaderCaching = true;
            }
        }

        [Fact]
        public void ParsingRdfABadProfile()
        {
            RdfAParser parser = new RdfAParser(RdfASyntax.RDFa_1_1);
            parser.Warning += TestTools.WarningPrinter;

            Graph g = new Graph();
            parser.Load(g, "resources\\bad_profile.xhtml");

            TestTools.ShowGraph(g);

            Assert.Equal(1, g.Triples.Count);
        }
    }
}
